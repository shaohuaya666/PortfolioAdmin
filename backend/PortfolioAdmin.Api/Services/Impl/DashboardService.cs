using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly PortfolioDbContext _db;

    public DashboardService(PortfolioDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        return new DashboardStats
        {
            ProjectCount = await _db.CompactProjects.CountAsync(),
            SkillCount = await _db.Tags.CountAsync(),
            WorkYearCount = await _db.WorkHistories.CountAsync(),
            DiagnosticCount = await _db.SkillDiagnostics.CountAsync()
        };
    }
}
