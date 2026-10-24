using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace PortfolioAdmin.Api.Caching;

/// <summary>
/// 基于 StackExchange.Redis 的前台缓存失效器实现。
/// </summary>
/// <remarks>
/// 设计要点：<br/>
/// 1. 连接延迟到首次使用（Lazy），Redis 不可用不会拖慢或阻断管理端启动；<br/>
/// 2. <c>AbortOnConnectFail = false</c>，Redis 未就绪时连接对象仍可创建，由 IsConnected 判定后跳过删除；<br/>
/// 3. 完整键在构造期一次性算好，运行期不再拼字符串；<br/>
/// 4. 任何异常都被吞掉并记警告——缓存失效失败只会导致前台最多多读 30 分钟旧数据，
///    不能让它反过来搞挂管理端的写操作。
/// </remarks>
public sealed class BlazorPortfolioCacheInvalidator : IBlazorPortfolioCacheInvalidator, IDisposable
{
    private readonly BlazorPortfolioCacheOptions _options;
    private readonly ILogger<BlazorPortfolioCacheInvalidator> _logger;
    private readonly Lazy<ConnectionMultiplexer?> _connection;

    private readonly RedisKey _advantages;
    private readonly RedisKey _skillCategories;
    private readonly RedisKey _workHistories;
    private readonly RedisKey _projects;
    private readonly RedisKey _skillDiagnostics;
    private readonly RedisKey[] _allKeys;

    private bool _disposed;

    public BlazorPortfolioCacheInvalidator(
        IOptions<BlazorPortfolioCacheOptions> options,
        ILogger<BlazorPortfolioCacheInvalidator> logger)
    {
        _options = options.Value;
        _logger = logger;

        _advantages = Build(BlazorPortfolioCacheKeys.Advantages);
        _skillCategories = Build(BlazorPortfolioCacheKeys.SkillCategories);
        _workHistories = Build(BlazorPortfolioCacheKeys.WorkHistories);
        _projects = Build(BlazorPortfolioCacheKeys.CompactProjects);
        _skillDiagnostics = Build(BlazorPortfolioCacheKeys.SkillDiagnostics);
        _allKeys = new[] { _advantages, _skillCategories, _workHistories, _projects, _skillDiagnostics };

        _connection = new Lazy<ConnectionMultiplexer?>(CreateConnection, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private RedisKey Build(string businessKey)
        => BlazorPortfolioCacheKeys.Build(_options.KeyPrefix, _options.KeyVersion, businessKey);

    /// <inheritdoc />
    public Task InvalidateAdvantagesAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(new[] { _advantages }, cancellationToken);

    /// <inheritdoc />
    public Task InvalidateSkillCategoriesAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(new[] { _skillCategories }, cancellationToken);

    /// <inheritdoc />
    public Task InvalidateWorkHistoriesAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(new[] { _workHistories }, cancellationToken);

    /// <inheritdoc />
    public Task InvalidateProjectsAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(new[] { _projects }, cancellationToken);

    /// <inheritdoc />
    public Task InvalidateSkillDiagnosticsAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(new[] { _skillDiagnostics }, cancellationToken);

    /// <inheritdoc />
    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
        => RemoveAsync(_allKeys, cancellationToken);

    /// <summary>
    /// 删除指定缓存键。Redis 不可用时静默返回，异常情况记录警告后返回。
    /// </summary>
    /// <param name="keys">待删除的完整缓存键</param>
    /// <param name="cancellationToken">调用方取消令牌</param>
    private async Task RemoveAsync(RedisKey[] keys, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            var db = GetDatabase();
            if (db is null)
            {
                _logger.LogWarning("[前台缓存失效] Redis 当前不可用，已跳过删除：{Keys}",
                    string.Join(", ", keys.Select(k => (string?)k)));
                return;
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(_options.OperationTimeoutMs));

            var deleted = await db.KeyDeleteAsync(keys).WaitAsync(timeoutCts.Token).ConfigureAwait(false);

            _logger.LogInformation("[前台缓存失效] 已删除 {Deleted}/{Total} 个键：{Keys}",
                deleted, keys.Length, string.Join(", ", keys.Select(k => (string?)k)));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[前台缓存失效] 删除超时（>{Timeout}ms），已放弃：{Keys}",
                _options.OperationTimeoutMs, string.Join(", ", keys.Select(k => (string?)k)));
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "[前台缓存失效] Redis 操作失败，前台将在 TTL（默认 30 分钟）到期后自动恢复：{Keys}",
                string.Join(", ", keys.Select(k => (string?)k)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[前台缓存失效] 发生未预期异常，已忽略：{Keys}",
                string.Join(", ", keys.Select(k => (string?)k)));
        }
    }

    /// <summary>
    /// 获取数据库句柄；连接不可用时返回 null（不抛异常，交由调用方跳过）。
    /// </summary>
    private IDatabase? GetDatabase()
    {
        var connection = _connection.Value;
        if (connection is null || !connection.IsConnected)
        {
            return null;
        }

        return connection.GetDatabase(_options.Redis.Database);
    }

    private ConnectionMultiplexer? CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_options.Redis.ConnectionString))
        {
            _logger.LogWarning("[前台缓存失效] 未配置 Cache:Redis:ConnectionString，缓存失效功能已禁用。");
            return null;
        }

        try
        {
            var configuration = ConfigurationOptions.Parse(_options.Redis.ConnectionString);
            configuration.AbortOnConnectFail = false; // 必须为 false：Redis 未就绪不能阻断管理端运行
            configuration.ConnectTimeout = _options.Redis.ConnectTimeoutMs;
            configuration.SyncTimeout = _options.Redis.SyncTimeoutMs;
            configuration.ConnectRetry = _options.Redis.ConnectRetry;
            configuration.ClientName = "PortfolioAdmin";
            configuration.DefaultDatabase = _options.Redis.Database;

            var connection = ConnectionMultiplexer.Connect(configuration);

            connection.ConnectionFailed += (_, e) =>
                _logger.LogWarning("[前台缓存失效] Redis 连接失败：{EndPoint} - {FailureType}，暂时跳过缓存失效。",
                    e.EndPoint, e.FailureType);
            connection.ConnectionRestored += (_, _) =>
                _logger.LogInformation("[前台缓存失效] Redis 连接已恢复。");

            _logger.LogInformation(
                "[前台缓存失效] 已连接 Redis {EndPoints}，数据库 {Database}，键命名空间 {Prefix}:{Version}。",
                string.Join(",", configuration.EndPoints), _options.Redis.Database,
                _options.KeyPrefix, _options.KeyVersion);

            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[前台缓存失效] Redis 初始化失败，后续缓存失效调用将自动跳过。");
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_connection.IsValueCreated)
        {
            _connection.Value?.Dispose();
        }
    }
}
