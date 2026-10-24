namespace PortfolioAdmin.Api.Caching;

/// <summary>
/// 前台站点（BlazorPortfolio）缓存失效器。<br/>
/// 管理端修改共享数据库中的展示数据后，通过它删除前台对应的缓存键，
/// 使前台下一次读取时按 Cache-Aside 逻辑重新回源。
/// </summary>
/// <remarks>
/// <para>所有方法都是<strong>尽力而为</strong>的：Redis 不可用时记录警告并直接返回，
/// 绝不抛异常，避免缓存问题影响管理端业务写入（数据库事务已提交，数据不会丢）。</para>
/// <para>只删除前台拥有的业务缓存键；管理端自身的权限/用户等数据不经缓存，不在处理范围。</para>
/// </remarks>
public interface IBlazorPortfolioCacheInvalidator
{
    /// <summary>失效「个人优势」聚合，对应键 <c>BlazorPortfolio:v1:Portfolio_Advantages</c></summary>
    Task InvalidateAdvantagesAsync(CancellationToken cancellationToken = default);

    /// <summary>失效「技术栈分类 + 标签」聚合，对应键 <c>BlazorPortfolio:v1:Portfolio_SkillCategories</c></summary>
    Task InvalidateSkillCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>失效「工作经历 + 成就」聚合，对应键 <c>BlazorPortfolio:v1:Portfolio_WorkHistories</c></summary>
    Task InvalidateWorkHistoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>失效「项目经历 + 项目技能」聚合，对应键 <c>BlazorPortfolio:v1:Portfolio_CompactProjects</c></summary>
    Task InvalidateProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>失效「技能诊断」聚合，对应键 <c>BlazorPortfolio:v1:Portfolio_SkillDiagnostics</c></summary>
    Task InvalidateSkillDiagnosticsAsync(CancellationToken cancellationToken = default);

    /// <summary>失效前台全部业务缓存键，用于批量导入等无法逐条判断的场景</summary>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Redis 当前是否可用，仅供健康检查使用，不读写任何数据。<br/>
    /// 缓存失效是尽力而为的，因此它只作为部署期的提示信息，不应作为可用性判定依据。
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
