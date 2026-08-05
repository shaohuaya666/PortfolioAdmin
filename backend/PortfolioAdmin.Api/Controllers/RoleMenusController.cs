using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.Attributes;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleMenusController : ControllerBase
{
    private readonly IRoleMenuService _service;

    public RoleMenusController(IRoleMenuService service) => _service = service;

    [HttpGet("{roleId}")]
    public async Task<ActionResult<RoleMenusResponse>> GetByRole(int roleId)
    {
        var result = await _service.GetRoleMenusAsync(roleId);
        return result is null ? NotFound(new { message = "角色不存在" }) : Ok(result);
    }

    [HttpPost]
    [RequirePermission("roles:assign_menus")]
    public async Task<IActionResult> Assign([FromBody] RoleMenuAssignRequest req)
    {
        var success = await _service.AssignRoleMenusAsync(req);
        return success ? NoContent() : NotFound(new { message = "角色不存在" });
    }
}
