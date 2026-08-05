using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.Attributes;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto is null ? NotFound(new { message = "用户不存在" }) : Ok(dto);
    }

    [HttpPost]
    [RequirePermission("users:create")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        var (success, message, data) = await _service.CreateAsync(req);
        if (!success)
        {
            if (message == "用户名已存在") return Conflict(new { message });
            return BadRequest(new { message });
        }
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [HttpPut("{id}")]
    [RequirePermission("users:edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var (success, message) = await _service.UpdateAsync(id, req);
        if (!success)
        {
            if (message == "用户不存在") return NotFound(new { message });
            if (message == "用户名已存在") return Conflict(new { message });
            return BadRequest(new { message });
        }
        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    [RequirePermission("users:reset_password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest req)
    {
        var (success, message) = await _service.ResetPasswordAsync(id, req.NewPassword);
        return success ? Ok(new { message }) : NotFound(new { message });
    }

    [HttpDelete("{id}")]
    [RequirePermission("users:delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound(new { message = "用户不存在" });
    }
}
