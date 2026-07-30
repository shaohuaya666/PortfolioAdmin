using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Middleware;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly JwtHelper _jwtHelper;

    public AuthController(IConfiguration config, JwtHelper jwtHelper)
    {
        _config = config;
        _jwtHelper = jwtHelper;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var adminUser = _config["AdminUser:Username"];
        var adminPass = _config["AdminUser:Password"];

        if (request.Username == adminUser && request.Password == adminPass)
        {
            var token = _jwtHelper.GenerateToken(request.Username);
            var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "480");
            return Ok(new LoginResponse
            {
                Token = token,
                Username = request.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes)
            });
        }

        return Unauthorized(new { message = "用户名或密码错误" });
    }
}
