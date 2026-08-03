using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleMenusController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public RoleMenusController(PortfolioDbContext db) => _db = db;

    /// <summary>
    /// 获取角色已分配的菜单ID列表
    /// </summary>
    [HttpGet("{roleId}")]
    public async Task<ActionResult<RoleMenusResponse>> GetByRole(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role == null) return NotFound(new { message = "角色不存在" });

        var menuIds = await _db.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        return Ok(new RoleMenusResponse { RoleId = roleId, MenuIds = menuIds });
    }

    /// <summary>
    /// 为角色分配菜单权限（全量替换）
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] RoleMenuAssignRequest req)
    {
        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null) return NotFound(new { message = "角色不存在" });

        // 清除现有分配
        var existing = await _db.RoleMenus.Where(rm => rm.RoleId == req.RoleId).ToListAsync();
        _db.RoleMenus.RemoveRange(existing);

        // 添加新分配
        var newAssigns = req.MenuIds.Select(menuId => new RoleMenu
        {
            RoleId = req.RoleId,
            MenuId = menuId
        });
        _db.RoleMenus.AddRange(newAssigns);

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
