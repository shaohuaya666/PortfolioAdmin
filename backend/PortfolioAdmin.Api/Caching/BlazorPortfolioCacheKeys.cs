using System.Text.RegularExpressions;

namespace PortfolioAdmin.Api.Caching;

/// <summary>
/// BlazorPortfolio 站点所使用的缓存键定义。<br/>
/// </summary>
/// <remarks>
/// <para>
/// 业务键字面量与 BlazorPortfolio 中 <c>PortfolioService</c> 传入
/// <c>CacheService.GetOrSetAsync</c> 的业务键一一对应；完整键由 <see cref="Build"/> 按
/// <c>{前缀}:{版本}:{业务键}</c> 组装，规则与 BlazorPortfolio 的 <c>CacheKeys.Build</c> 对齐。
/// </para>
/// <para>
/// 前台这 5 个键缓存的都是<strong>整体集合</strong>（含 Include 出来的子表），
/// 因此本项目的任何一次增删改都只做「整键删除」，不做增量改写。
/// </para>
/// </remarks>
public static class BlazorPortfolioCacheKeys
{
    /// <summary>个人优势 <c>List&lt;Advantage&gt;</c>，来源表 Advantages</summary>
    public const string Advantages = "Portfolio_Advantages";

    /// <summary>技术栈分类 <c>List&lt;SkillCategory&gt;</c>（Include Tags），来源表 SkillCategories + Tags</summary>
    public const string SkillCategories = "Portfolio_SkillCategories";

    /// <summary>工作经历 <c>List&lt;WorkHistory&gt;</c>（Include Achievements），来源表 WorkHistories + Achievements</summary>
    public const string WorkHistories = "Portfolio_WorkHistories";

    /// <summary>项目经历 <c>List&lt;CompactProject&gt;</c>（Include Skills），来源表 CompactProjects + ProjectSkills</summary>
    public const string CompactProjects = "Portfolio_CompactProjects";

    /// <summary>
    /// 技能诊断 <c>Dictionary&lt;string, SkillDiagnostic&gt;</c>（以 TagName 为字典键），来源表 SkillDiagnostics。
    /// </summary>
    public const string SkillDiagnostics = "Portfolio_SkillDiagnostics";

    /// <summary>
    /// 全部前台业务缓存键。
    /// 不含 <c>:lock</c> 分布式锁键——锁键自带 TTL 且仅在回源瞬间存在，删除它反而会破坏击穿防护。
    /// </summary>
    public static readonly string[] All =
    {
        Advantages,
        SkillCategories,
        WorkHistories,
        CompactProjects,
        SkillDiagnostics
    };

    /// <summary>缓存键最大长度限制，与 BlazorPortfolio 保持一致</summary>
    private const int MaxKeyLength = 256;

    /// <summary>允许出现在键中的字符：字母、数字、下划线、中划线、英文冒号、英文点</summary>
    private static readonly Regex InvalidCharacters = new(@"[^A-Za-z0-9_\-.:]", RegexOptions.Compiled);

    /// <summary>
    /// 按 <c>{前缀}:{版本}:{业务键}</c> 生成完整缓存键，规则与 BlazorPortfolio 的 <c>CacheKeys.Build</c> 完全一致。
    /// </summary>
    /// <param name="prefix">全局前缀</param>
    /// <param name="version">键版本</param>
    /// <param name="businessKey">业务键</param>
    /// <returns>完整缓存键</returns>
    public static string Build(string prefix, string version, string businessKey)
    {
        if (string.IsNullOrWhiteSpace(businessKey))
        {
            throw new ArgumentException("缓存业务键不能为空。", nameof(businessKey));
        }

        var business = InvalidCharacters.Replace(businessKey.Trim(), "_");
        var versionPart = string.IsNullOrWhiteSpace(version) ? "v1" : version.Trim();
        var prefixPart = string.IsNullOrWhiteSpace(prefix) ? "App" : prefix.Trim();

        var key = $"{prefixPart}:{versionPart}:{business}";

        if (key.Length > MaxKeyLength)
        {
            throw new ArgumentException($"缓存键长度超过上限 {MaxKeyLength}：{key}", nameof(businessKey));
        }

        return key;
    }
}
