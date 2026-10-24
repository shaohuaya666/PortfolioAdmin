namespace PortfolioAdmin.Api.Caching;

/// <summary>
/// 前台站点（BlazorPortfolio）缓存失效配置，对应 appsettings.json 的 "Cache" 节点。<br/>
/// 节点结构与 BlazorPortfolio 的 <c>Cache</c> 节点保持同构，两个项目可共用同一份
/// 环境变量（<c>Cache__KeyPrefix</c> / <c>Cache__KeyVersion</c> / <c>Cache__Redis__ConnectionString</c> …）。
/// </summary>
/// <remarks>
/// <strong>关键约束：</strong> <see cref="KeyPrefix"/> 与 <see cref="KeyVersion"/> 必须与 BlazorPortfolio
/// 的 <c>Cache:KeyPrefix</c> / <c>Cache:KeyVersion</c> 完全一致，否则删除的键名不同，
/// Redis 会返回"删除 0 个键"且不报错，导致前台页面继续读到旧数据却毫无提示。
/// </remarks>
public class BlazorPortfolioCacheOptions
{
    /// <summary>配置节点名称</summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// 是否启用缓存失效。关闭后所有失效调用退化为空操作，便于在前台未接入 Redis 时静默降级。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>全局缓存键前缀，必须与 BlazorPortfolio 一致</summary>
    public string KeyPrefix { get; set; } = "BlazorPortfolio";

    /// <summary>缓存键版本号，必须与 BlazorPortfolio 一致</summary>
    public string KeyVersion { get; set; } = "v1";

    /// <summary>单次失效操作的最长等待时间（毫秒）；超时即放弃，不阻塞管理端写入流程</summary>
    public int OperationTimeoutMs { get; set; } = 3000;

    /// <summary>Redis 连接配置，与 BlazorPortfolio 的 <c>Cache:Redis</c> 节点同构</summary>
    public RedisInvalidationOptions Redis { get; set; } = new();
}

/// <summary>用于删除前台缓存键的 Redis 连接配置</summary>
public class RedisInvalidationOptions
{
    /// <summary>连接字符串，支持 StackExchange.Redis 语法，例：cache:6379,password=xxx,ssl=false</summary>
    public string ConnectionString { get; set; } = "127.0.0.1:6379";

    /// <summary>数据库编号，必须与 BlazorPortfolio 的 <c>Cache:Redis:Database</c> 一致</summary>
    public int Database { get; set; }

    /// <summary>连接超时（毫秒）</summary>
    public int ConnectTimeoutMs { get; set; } = 3000;

    /// <summary>同步操作超时（毫秒）</summary>
    public int SyncTimeoutMs { get; set; } = 3000;

    /// <summary>初始连接失败时的重试次数</summary>
    public int ConnectRetry { get; set; } = 2;
}
