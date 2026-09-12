namespace PortfolioAdmin.Api.DTOs;

public class CaptchaData
{
    public string CaptchaId { get; set; } = string.Empty;
    public int PuzzleWidth { get; set; } = 50;
    public int PuzzleHeight { get; set; } = 50;
    public int BgWidth { get; set; } = 300;
    public int BgHeight { get; set; } = 160;
    public int TargetX { get; set; }
    public int TargetY { get; set; } = 55;
}

public class CaptchaVerifyRequest
{
    public string CaptchaId { get; set; } = string.Empty;
    public int SliderOffset { get; set; }
    public List<CaptchaTrackPoint> TrackData { get; set; } = new();
}

public class CaptchaTrackPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public long Timestamp { get; set; }
}
