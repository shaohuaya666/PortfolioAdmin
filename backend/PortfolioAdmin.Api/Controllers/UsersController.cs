using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Utils;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public UsersController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role!.Name,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound(new { message = "用户不存在" });

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "用户名已存在" });

        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null) return BadRequest(new { message = "角色不存在" });

        var user = new User
        {
            Username = req.Username,
            PasswordHash = PasswordHelper.Hash(req.Password),
            RoleId = req.RoleId,
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = role.Name,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "用户不存在" });

        if (await _db.Users.AnyAsync(u => u.Username == req.Username && u.Id != id))
            return Conflict(new { message = "用户名已存在" });

        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null) return BadRequest(new { message = "角色不存在" });

        user.Username = req.Username;
        user.RoleId = req.RoleId;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "用户不存在" });

        user.PasswordHash = PasswordHelper.Hash(req.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "密码重置成功" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "用户不存在" });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
