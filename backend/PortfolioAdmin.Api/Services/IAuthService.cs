using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> ChangePasswordAsync(string username, ChangePasswordRequest request);
}
