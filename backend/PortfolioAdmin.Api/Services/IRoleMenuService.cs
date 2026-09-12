using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public interface IRoleMenuService
{
    // ============ Roles ============
    Task<List<RoleDto>> GetRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<(bool Success, string Message, RoleDto? Data)> CreateRoleAsync(CreateRoleRequest req);
    Task<(bool Success, string Message)> UpdateRoleAsync(int id, CreateRoleRequest req);
    Task<(bool Success, string Message)> DeleteRoleAsync(int id);

    // ============ Menus ============
    Task<List<MenuTreeNode>> GetMenuTreeAsync();
    Task<List<MenuTreeNode>> GetMenuTreeWithActionsAsync();
    Task<List<MenuDto>> GetMyMenusAsync(string username);
    Task<MenuTreeNode?> CreateMenuAsync(CreateMenuRequest req);
    Task<bool> UpdateMenuAsync(int id, CreateMenuRequest req);
    Task<bool> DeleteMenuAsync(int id);

    // ============ RoleMenus ============
    Task<RoleMenusResponse?> GetRoleMenusAsync(int roleId);
    Task<bool> AssignRoleMenusAsync(RoleMenuAssignRequest req);

    // ============ Permissions ============
    Task<List<string>> GetMyPermissionsAsync(string username);
}
