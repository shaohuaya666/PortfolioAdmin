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
    public string RoleName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<MenuDto> Menus { get; set; } = new();
}

public class MenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int ParentId { get; set; }
    public int Sort { get; set; }
    public List<MenuDto> Children { get; set; } = new();
}

public class ChangePasswordRequest
{
    public string Username { get; set; } = string.Empty;
    [Required]
    public string OldPassword { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

public class DashboardStats
{
    public int ProjectCount { get; set; }
    public int SkillCount { get; set; }
    public int WorkYearCount { get; set; }
    public int DiagnosticCount { get; set; }
}
