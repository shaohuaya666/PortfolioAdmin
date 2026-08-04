using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleMenuService _service;

    public RolesController(IRoleMenuService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAll()
        => Ok(await _service.GetRolesAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var dto = await _service.GetRoleByIdAsync(id);
        return dto is null ? NotFound(new { message = "角色不存在" }) : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleRequest req)
    {
        var (success, message, data) = await _service.CreateRoleAsync(req);
        if (!success)
            return Conflict(new { message });
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoleRequest req)
    {
        var (success, message) = await _service.UpdateRoleAsync(id, req);
        if (!success)
        {
            if (message == "角色不存在") return NotFound(new { message });
            return Conflict(new { message });
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteRoleAsync(id);
        if (!success)
        {
            if (message == "角色不存在") return NotFound(new { message });
            return BadRequest(new { message });
        }
        return NoContent();
    }
}
