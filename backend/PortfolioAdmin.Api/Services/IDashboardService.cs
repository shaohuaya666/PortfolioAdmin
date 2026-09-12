using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync();
}
