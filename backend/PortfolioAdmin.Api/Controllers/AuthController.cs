using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.Attributes;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "用户名和密码不能为空" });

        var (result, errorMessage) = await _auth.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = errorMessage ?? "用户名或密码错误" });
        return result;
    }

    [HttpPost("change-password")]
    [RequirePermission("users:reset_password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return Unauthorized(new { message = "未找到用户身份信息" });
            username = request.Username;
        }

        var (success, message) = await _auth.ChangePasswordAsync(username, request);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}
