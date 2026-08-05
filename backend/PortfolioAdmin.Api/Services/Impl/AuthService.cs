using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Middleware;
using PortfolioAdmin.Api.Utils;

namespace PortfolioAdmin.Api.Services;

public class AuthService : IAuthService
{
    private readonly PortfolioDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICaptchaService _captcha;

    public AuthService(PortfolioDbContext db, IJwtService jwt, IConfiguration config, IPasswordHasher passwordHasher, ICaptchaService captcha)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
        _passwordHasher = passwordHasher;
        _captcha = captcha;
    }

    public async Task<(LoginResponse? Result, string? ErrorMessage)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return (null, "用户名和密码不能为空");

        // 验证码强制校验
        if (string.IsNullOrEmpty(request.CaptchaId))
            return (null, "请先完成滑块验证");
        if (!_captcha.IsVerified(request.CaptchaId))
            return (null, "滑块验证已过期，请重新验证");

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return (null, "用户名或密码错误");

        // 登录成功后才消耗验证码（避免输错密码后需要重新滑）
        _captcha.Consume(request.CaptchaId);

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
        var token = _jwt.GenerateToken(user.Username);
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "480");

        return (new LoginResponse
        {
            Token = token,
            Username = user.Username,
            RoleName = user.Role?.Name ?? "",
            ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            Menus = menuTree
        }, null);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string username, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return (false, "旧密码和新密码不能为空");

        if (request.NewPassword.Length < 6)
            return (false, "新密码长度不能少于6位");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            return (false, "用户不存在");

        if (!_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
            return (false, "旧密码错误");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync();

        return (true, "密码修改成功");
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
