using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioAdmin.Api.Caching;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.Middleware;
using PortfolioAdmin.Api.Services;
using PortfolioAdmin.Api.Utils;

var builder = WebApplication.CreateBuilder(args);

// CORS - 允许前端开发服务器访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PortfolioAdmin API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT 认证
// 密钥必须来自环境变量 Jwt__SecretKey，严禁写进 appsettings.json（会随镜像进 Git）。
// 缺失时快速失败，避免以空密钥/弱密钥启动后在登录阶段才暴露问题。
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || Encoding.UTF8.GetByteCount(jwtSecretKey) < 32)
{
    if (!builder.Environment.IsDevelopment())
    {
        // 生产环境必须显式提供，避免以空密钥/弱密钥启动后在登录阶段才暴露问题
        throw new InvalidOperationException(
            "Jwt:SecretKey 未配置或长度不足 32 字节，请通过环境变量 Jwt__SecretKey 配置。");
    }

    // 仅本地开发使用占位密钥，不影响任何线上环境。
    // 必须写回配置：JwtHelper 签发时独立读取 Jwt:SecretKey，不写回会导致开发环境下登录抛空引用。
    jwtSecretKey = "PortfolioAdmin-Development-Only-Key-0123456789";
    builder.Configuration["Jwt:SecretKey"] = jwtSecretKey;
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // 与 JwtHelper 签发端共用同一解析逻辑，杜绝"签发与校验不一致"导致的全站 401
            ValidIssuer = JwtHelper.ResolveIssuer(builder.Configuration),
            ValidAudience = JwtHelper.ResolveAudience(builder.Configuration),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 前台站点（BlazorPortfolio）缓存失效：与管理端共用同一份 Cache 配置结构
builder.Services.Configure<BlazorPortfolioCacheOptions>(
    builder.Configuration.GetSection(BlazorPortfolioCacheOptions.SectionName));
builder.Services.AddSingleton<IBlazorPortfolioCacheInvalidator, BlazorPortfolioCacheInvalidator>();

// 注册业务服务
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleMenuService, RoleMenuService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 注册工具/基础设施服务
builder.Services.AddSingleton<IPasswordHasher, PasswordHelper>();
builder.Services.AddSingleton<IJwtService, JwtHelper>();
builder.Services.AddSingleton<ICaptchaService, CaptchaService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
