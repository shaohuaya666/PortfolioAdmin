using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public RolesController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAll()
    {
        var roles = await _db.Roles
            .OrderBy(r => r.Id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return NotFound(new { message = "角色不存在" });

        return Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleRequest req)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == req.Name))
            return Conflict(new { message = "角色名已存在" });

        var role = new Role
        {
            Name = req.Name,
            Description = req.Description,
            CreatedAt = DateTime.Now
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoleRequest req)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return NotFound(new { message = "角色不存在" });

        if (await _db.Roles.AnyAsync(r => r.Name == req.Name && r.Id != id))
            return Conflict(new { message = "角色名已存在" });

        role.Name = req.Name;
        role.Description = req.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _db.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return NotFound(new { message = "角色不存在" });
        if (role.Users.Count > 0) return BadRequest(new { message = "该角色下还有用户，无法删除" });

        _db.RoleMenus.RemoveRange(await _db.RoleMenus.Where(rm => rm.RoleId == id).ToListAsync());
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
