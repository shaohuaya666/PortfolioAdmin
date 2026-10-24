using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Caching;
using PortfolioAdmin.Api.Data;

namespace PortfolioAdmin.Api.Controllers;

/// <summary>
/// 部署冒烟用的健康检查端点，允许匿名访问。
/// </summary>
/// <remarks>
/// <para>存在的必要性：EF Core 是懒连接，数据库连不上时后端依然能正常启动，
/// 仅凭"容器处于 running"或"前端返回 200"都发现不了问题，只有真实请求进来才 500。
/// 流水线在 <c>up -d</c> 之后调用本端点，让"连接串配错"在部署阶段就直接失败，
/// 而不是等用户登录时才发现。</para>
/// <para>只回显连通性结论，不返回任何连接信息。</para>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    private readonly IBlazorPortfolioCacheInvalidator _cache;

    public HealthController(PortfolioDbContext db, IBlazorPortfolioCacheInvalidator cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>
    /// 数据库可用返回 200；不可用返回 503。
    /// Redis 只作为提示字段：缓存失效本身是尽力而为的，不应阻断部署。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var databaseReachable = await CanConnectAsync(cancellationToken);
        var cacheReachable = await _cache.IsAvailableAsync(cancellationToken);

        var payload = new
        {
            status = databaseReachable ? "ok" : "unavailable",
            database = databaseReachable,
            cache = cacheReachable
        };

        return databaseReachable ? Ok(payload) : StatusCode(503, payload);
    }

    private async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
