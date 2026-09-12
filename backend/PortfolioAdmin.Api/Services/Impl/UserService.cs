using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Utils;

namespace PortfolioAdmin.Api.Services;

public class UserService : IUserService
{
    private readonly PortfolioDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(PortfolioDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _db.Users
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
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<(bool Success, string Message, UserDto? Data)> CreateAsync(CreateUserRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return (false, "用户名已存在", null);

        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null)
            return (false, "角色不存在", null);

        var user = new User
        {
            Username = req.Username,
            PasswordHash = _passwordHasher.Hash(req.Password),
            RoleId = req.RoleId,
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = role.Name,
            CreatedAt = user.CreatedAt
        };

        return (true, "", dto);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateUserRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return (false, "用户不存在");

        if (await _db.Users.AnyAsync(u => u.Username == req.Username && u.Id != id))
            return (false, "用户名已存在");

        var role = await _db.Roles.FindAsync(req.RoleId);
        if (role == null)
            return (false, "角色不存在");

        user.Username = req.Username;
        user.RoleId = req.RoleId;
        await _db.SaveChangesAsync();

        return (true, "");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(int id, string newPassword)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return (false, "用户不存在");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _db.SaveChangesAsync();

        return (true, "密码重置成功");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
