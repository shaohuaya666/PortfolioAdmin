using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Middleware;
using PortfolioAdmin.Api.Utils;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _config;

    public AuthController(PortfolioDbContext db, JwtHelper jwtHelper, IConfiguration config)
    {
        _db = db;
        _jwtHelper = jwtHelper;
        _config = config;
    }

    /// <summary>
    /// 登录 - 返回 Token + 角色 + 菜单树
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "用户名和密码不能为空" });

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !PasswordHelper.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "用户名或密码错误" });

        // 获取用户角色的菜单权限
        var menuIds = await _db.RoleMenus
            .Where(rm => rm.RoleId == user.RoleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        var allMenus = await _db.Menus
            .Where(m => menuIds.Contains(m.Id))
            .OrderBy(m => m.Sort)
            .ToListAsync();

        var menuDtos = allMenus.Select(m => new MenuDto
        {
            Id = m.Id,
            Name = m.Name,
            Path = m.Path,
            Icon = m.Icon,
            ParentId = m.ParentId,
            Sort = m.Sort
        }).ToList();

        var menuTree = BuildMenuTree(menuDtos, 0);

        var token = _jwtHelper.GenerateToken(user.Username);
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "480");

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            RoleName = user.Role?.Name ?? "",
            ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            Menus = menuTree
        };
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return Unauthorized(new { message = "未找到用户身份信息" });
            username = request.Username;
        }

        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "旧密码和新密码不能为空" });

        if (request.NewPassword.Length < 6)
            return BadRequest(new { message = "新密码长度不能少于6位" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            return NotFound(new { message = "用户不存在" });

        if (!PasswordHelper.Verify(request.OldPassword, user.PasswordHash))
            return BadRequest(new { message = "旧密码错误" });

        user.PasswordHash = PasswordHelper.Hash(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "密码修改成功" });
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
