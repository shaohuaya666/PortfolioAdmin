using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Services;

public class RoleMenuService : IRoleMenuService
{
    private readonly PortfolioDbContext _db;

    public RoleMenuService(PortfolioDbContext db)
    {
        _db = db;
    }

    // ============ Roles ============
    public async Task<List<RoleDto>> GetRolesAsync()
    {
        return await _db.Roles
            .OrderBy(r => r.Id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };
    }

    public async Task<(bool Success, string Message, RoleDto? Data)> CreateRoleAsync(CreateRoleRequest req)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == req.Name))
            return (false, "角色名已存在", null);

        var role = new Role
        {
            Name = req.Name,
            Description = req.Description,
            CreatedAt = DateTime.Now
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        return (true, "", dto);
    }

    public async Task<(bool Success, string Message)> UpdateRoleAsync(int id, CreateRoleRequest req)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null)
            return (false, "角色不存在");

        if (await _db.Roles.AnyAsync(r => r.Name == req.Name && r.Id != id))
            return (false, "角色名已存在");

        role.Name = req.Name;
        role.Description = req.Description;
        await _db.SaveChangesAsync();

        return (true, "");
    }

    public async Task<(bool Success, string Message)> DeleteRoleAsync(int id)
    {
        var role = await _db.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return (false, "角色不存在");
        if (role.Users.Count > 0)
            return (false, "该角色下还有用户，无法删除");

        var roleMenus = await _db.RoleMenus.Where(rm => rm.RoleId == id).ToListAsync();
        _db.RoleMenus.RemoveRange(roleMenus);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        return (true, "");
    }

    // ============ Menus ============
    public async Task<List<MenuTreeNode>> GetMenuTreeAsync()
    {
        var menus = await _db.Menus
            .OrderBy(m => m.Sort)
            .Select(m => new MenuTreeNode
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return BuildTree(menus, 0);
    }

    public async Task<List<MenuDto>> GetMyMenusAsync(string username)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return new List<MenuDto>();

        var menuIds = await _db.RoleMenus
            .Where(rm => rm.RoleId == user.RoleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        var menus = await _db.Menus
            .Where(m => menuIds.Contains(m.Id))
            .OrderBy(m => m.Sort)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort
            })
            .ToListAsync();

        return BuildMenuTree(menus, 0);
    }

    public async Task<MenuTreeNode?> CreateMenuAsync(CreateMenuRequest req)
    {
        var menu = new Menu
        {
            Name = req.Name,
            Path = req.Path,
            Icon = req.Icon,
            ParentId = req.ParentId,
            Sort = req.Sort,
            CreatedAt = DateTime.Now
        };

        _db.Menus.Add(menu);
        await _db.SaveChangesAsync();

        return new MenuTreeNode
        {
            Id = menu.Id,
            Name = menu.Name,
            Path = menu.Path,
            Icon = menu.Icon,
            ParentId = menu.ParentId,
            Sort = menu.Sort,
            CreatedAt = menu.CreatedAt
        };
    }

    public async Task<bool> UpdateMenuAsync(int id, CreateMenuRequest req)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return false;

        menu.Name = req.Name;
        menu.Path = req.Path;
        menu.Icon = req.Icon;
        menu.ParentId = req.ParentId;
        menu.Sort = req.Sort;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteMenuAsync(int id)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return false;

        var children = await _db.Menus.Where(m => m.ParentId == id).ToListAsync();
        _db.Menus.RemoveRange(children);

        var refs = await _db.RoleMenus.Where(rm => rm.MenuId == id).ToListAsync();
        _db.RoleMenus.RemoveRange(refs);

        _db.Menus.Remove(menu);
        await _db.SaveChangesAsync();

        return true;
    }

    // ============ RoleMenus ============
    public async Task<RoleMenusResponse?> GetRoleMenusAsync(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role == null) return null;

        var menuIds = await _db.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        return new RoleMenusResponse { RoleId = roleId, MenuIds = menuIds };
    }

    public async Task<bool> AssignRoleMenusAsync(RoleMenuAssignRequest req)
    {
        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null) return false;

        var existing = await _db.RoleMenus.Where(rm => rm.RoleId == req.RoleId).ToListAsync();
        _db.RoleMenus.RemoveRange(existing);

        var newAssigns = req.MenuIds.Select(menuId => new RoleMenu
        {
            RoleId = req.RoleId,
            MenuId = menuId
        });
        _db.RoleMenus.AddRange(newAssigns);

        await _db.SaveChangesAsync();
        return true;
    }

    // ============ Tree Builders ============
    private static List<MenuTreeNode> BuildTree(List<MenuTreeNode> menus, int parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.Sort)
            .Select(m => new MenuTreeNode
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort,
                CreatedAt = m.CreatedAt,
                Children = BuildTree(menus, m.Id)
            })
            .ToList();
    }

    private static List<MenuDto> BuildMenuTree(List<MenuDto> menus, int parentId)
    {
        return menus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.Sort)
            .Select(m => new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Path = m.Path,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Sort = m.Sort,
                Children = BuildMenuTree(menus, m.Id)
            })
            .ToList();
    }
}
