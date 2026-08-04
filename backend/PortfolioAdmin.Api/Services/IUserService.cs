using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, UserDto? Data)> CreateAsync(CreateUserRequest req);
    Task<(bool Success, string Message)> UpdateAsync(int id, UpdateUserRequest req);
    Task<(bool Success, string Message)> ResetPasswordAsync(int id, string newPassword);
    Task<bool> DeleteAsync(int id);
}
