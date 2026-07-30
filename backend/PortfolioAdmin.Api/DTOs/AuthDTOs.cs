using System.ComponentModel.DataAnnotations;

namespace PortfolioAdmin.Api.DTOs;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class DashboardStats
{
    public int ProjectCount { get; set; }
    public int SkillCount { get; set; }
    public int WorkYearCount { get; set; }
    public int DiagnosticCount { get; set; }
}
