namespace PortfolioAdmin.Api.Middleware;

public interface IJwtService
{
    string GenerateToken(string username);
}
