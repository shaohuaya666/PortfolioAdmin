using System.Collections.Concurrent;
using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public class CaptchaService : ICaptchaService
{
    private readonly ConcurrentDictionary<string, CaptchaSession> _sessions = new();
    private readonly Timer _cleanupTimer;

    // 容差范围（像素）
    private const int Tolerance = 5;
    // 验证码有效期（分钟）
    private const int ExpireMinutes = 5;
    // 最小滑动时间（毫秒），机器人通常极快
    private const int MinSlideDurationMs = 200;
    // 最少轨迹采样点数
    private const int MinTrackPoints = 3;

    private class CaptchaSession
    {
        public int TargetX { get; init; }
        public int TargetY { get; init; }
        public DateTime ExpireAt { get; init; }
        public bool Verified { get; set; }
    }

    public CaptchaService()
    {
        // 每分钟清理过期会话
        _cleanupTimer = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public CaptchaData Generate()
    {
        var id = Guid.NewGuid().ToString("N");
        var random = new Random();
        var bgWidth = 300;
        var puzzleWidth = 50;
        var puzzleHeight = 50;

        // 目标X在合理范围内：避免太靠左或太靠右
        var minX = puzzleWidth / 2 + 10;
        var maxX = bgWidth - puzzleWidth - 10;
        var targetX = random.Next(minX, maxX);
        var targetY = 55;

        _sessions[id] = new CaptchaSession
        {
            TargetX = targetX,
            TargetY = targetY,
            ExpireAt = DateTime.UtcNow.AddMinutes(ExpireMinutes),
            Verified = false
        };

        return new CaptchaData
        {
            CaptchaId = id,
            PuzzleWidth = puzzleWidth,
            PuzzleHeight = puzzleHeight,
            BgWidth = bgWidth,
            BgHeight = 160,
            TargetX = targetX,
            TargetY = targetY
        };
    }

    public (bool Success, string Message) Verify(CaptchaVerifyRequest request)
    {
        if (string.IsNullOrEmpty(request.CaptchaId))
            return (false, "验证码ID不能为空");

        if (!_sessions.TryGetValue(request.CaptchaId, out var session))
            return (false, "验证码已过期，请刷新重试");

        if (DateTime.UtcNow > session.ExpireAt)
        {
            _sessions.TryRemove(request.CaptchaId, out _);
            return (false, "验证码已过期，请刷新重试");
        }

        // 1. 滑动距离校验
        var distance = Math.Abs(request.SliderOffset - session.TargetX);
        if (distance > Tolerance)
            return (false, "验证失败，请重试");

        // 2. 轨迹数据校验
        var track = request.TrackData;
        if (track == null || track.Count < MinTrackPoints)
            return (false, "验证失败，请重试");

        // 3. 滑动时间校验
        var duration = track[^1].Timestamp - track[0].Timestamp;
        if (duration < MinSlideDurationMs)
            return (false, "验证失败，请重试");

        // 4. 轨迹平滑度校验（简单检测是否为机器人生成的直线轨迹）
        if (!IsHumanLikeTrack(track))
            return (false, "验证失败，请重试");

        // 验证通过
        session.Verified = true;
        return (true, "验证通过");
    }

    public bool IsVerified(string captchaId)
    {
        if (string.IsNullOrEmpty(captchaId)) return false;
        if (!_sessions.TryGetValue(captchaId, out var session)) return false;
        if (DateTime.UtcNow > session.ExpireAt)
        {
            _sessions.TryRemove(captchaId, out _);
            return false;
        }
        return session.Verified;
    }

    public void Consume(string captchaId)
    {
        if (!string.IsNullOrEmpty(captchaId))
            _sessions.TryRemove(captchaId, out _);
    }

    /// <summary>
    /// 类人轨迹检测：滑块为水平拖动，主要检测 X 轴合理性
    /// </summary>
    private static bool IsHumanLikeTrack(List<CaptchaTrackPoint> track)
    {
        if (track.Count < 3) return false;

        // 1. X 轴应该整体向右（正向）移动
        // 允许少量回退（人类微调），但回退次数不能超过总移动次数的 30%
        var backCount = 0;
        var totalMoves = 0;
        for (int i = 1; i < track.Count; i++)
        {
            if (track[i].X - track[i - 1].X != 0)
                totalMoves++;
            if (track[i].X < track[i - 1].X)
                backCount++;
        }
        if (totalMoves > 0 && (double)backCount / totalMoves > 0.3)
            return false;

        // 2. 如果所有时间间隔完全相同（精确到毫秒），疑似程序化操作
        // 浏览器 mousemove 事件间隔受事件循环影响会自然波动
        if (track.Count >= 6)
        {
            var intervals = new List<long>();
            for (int i = 1; i < track.Count; i++)
                intervals.Add(track[i].Timestamp - track[i - 1].Timestamp);

            if (intervals.Distinct().Count() == 1)
                return false;
        }

        return true;
    }

    private void CleanupExpired(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _sessions
            .Where(kv => now > kv.Value.ExpireAt)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in expiredKeys)
            _sessions.TryRemove(key, out _);
    }
}
