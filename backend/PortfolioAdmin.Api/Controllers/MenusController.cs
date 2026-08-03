using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public MenusController(PortfolioDbContext db) => _db = db;

    /// <summary>
    /// 获取完整菜单树
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<MenuTreeNode>>> GetTree()
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

        return Ok(BuildTree(menus, 0));
    }

    /// <summary>
    /// 获取当前用户可见菜单（已展平为树）
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<MenuDto>>> GetMyMenus()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "未登录" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return Unauthorized(new { message = "用户不存在" });

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

        return Ok(BuildMenuTree(menus, 0));
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuTreeNode>>> GetAll()
    {
        return await GetTree();
    }

    [HttpPost]
    public async Task<ActionResult<MenuTreeNode>> Create([FromBody] CreateMenuRequest req)
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

        return CreatedAtAction(nameof(GetAll), new { id = menu.Id }, new MenuTreeNode
        {
            Id = menu.Id,
            Name = menu.Name,
            Path = menu.Path,
            Icon = menu.Icon,
            ParentId = menu.ParentId,
            Sort = menu.Sort,
            CreatedAt = menu.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMenuRequest req)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return NotFound(new { message = "菜单不存在" });

        menu.Name = req.Name;
        menu.Path = req.Path;
        menu.Icon = req.Icon;
        menu.ParentId = req.ParentId;
        menu.Sort = req.Sort;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return NotFound(new { message = "菜单不存在" });

        // 删除子菜单
        var children = await _db.Menus.Where(m => m.ParentId == id).ToListAsync();
        _db.Menus.RemoveRange(children);

        // 删除角色菜单关联
        var refs = await _db.RoleMenus.Where(rm => rm.MenuId == id).ToListAsync();
        _db.RoleMenus.RemoveRange(refs);

        _db.Menus.Remove(menu);
        await _db.SaveChangesAsync();

        return NoContent();
    }

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
