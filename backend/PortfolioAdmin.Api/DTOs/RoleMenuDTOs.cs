using System.ComponentModel.DataAnnotations;

namespace PortfolioAdmin.Api.DTOs;

// ===== 角色 DTO =====
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
}

// ===== 菜单 DTO =====
public class MenuTreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int ParentId { get; set; }
    public int Sort { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MenuTreeNode> Children { get; set; } = new();
}

public class CreateMenuRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string Path { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Icon { get; set; }
    public int ParentId { get; set; }
    public int Sort { get; set; }
}

// ===== 角色菜单 DTO =====
public class RoleMenuAssignRequest
{
    [Required]
    public int RoleId { get; set; }
    [Required]
    public List<int> MenuIds { get; set; } = new();
}

public class RoleMenusResponse
{
    public int RoleId { get; set; }
    public List<int> MenuIds { get; set; } = new();
}

// ===== 用户管理 DTO =====
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public int RoleId { get; set; }
}

public class UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

public class ResetPasswordRequest
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
