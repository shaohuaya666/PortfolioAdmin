using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IRoleMenuService _service;

    public MenusController(IRoleMenuService service) => _service = service;

    [HttpGet("tree")]
    public async Task<ActionResult<List<MenuTreeNode>>> GetTree()
        => Ok(await _service.GetMenuTreeAsync());

    [HttpGet("my")]
    public async Task<ActionResult<List<MenuDto>>> GetMyMenus()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "未登录" });
        return Ok(await _service.GetMyMenusAsync(username));
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuTreeNode>>> GetAll()
        => Ok(await _service.GetMenuTreeAsync());

    [HttpPost]
    public async Task<ActionResult<MenuTreeNode>> Create([FromBody] CreateMenuRequest req)
    {
        var result = await _service.CreateMenuAsync(req);
        return result is null
            ? BadRequest()
            : CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMenuRequest req)
    {
        var success = await _service.UpdateMenuAsync(id, req);
        return success ? NoContent() : NotFound(new { message = "菜单不存在" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteMenuAsync(id);
        return success ? NoContent() : NotFound(new { message = "菜单不存在" });
    }
}
