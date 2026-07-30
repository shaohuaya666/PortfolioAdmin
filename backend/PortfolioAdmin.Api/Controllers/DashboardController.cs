using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public DashboardController(PortfolioDbContext db) => _db = db;

    [HttpGet("stats")]
    public async Task<DashboardStats> GetStats()
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
