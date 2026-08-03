using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.Middleware;
using PortfolioAdmin.Api.Models;
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
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PortfolioAdmin",
            ValidAudience = "PortfolioAdmin",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 注册服务
builder.Services.AddSingleton<JwtHelper>();

var app = builder.Build();

// ===== 数据库初始化：建 RBAC 表 + 种子数据 =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // 创建 RBAC 相关表（Users 表已由用户手动创建）
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Roles (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(50) NOT NULL,
                Description VARCHAR(200) NULL,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Menus (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(50) NOT NULL,
                Path VARCHAR(200) NOT NULL,
                Icon VARCHAR(100) NULL,
                ParentId INT NOT NULL DEFAULT 0,
                Sort INT NOT NULL DEFAULT 0,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS RoleMenus (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                RoleId INT NOT NULL,
                MenuId INT NOT NULL,
                FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
                FOREIGN KEY (MenuId) REFERENCES Menus(Id) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        // 种子管理员角色
        if (!db.Roles.Any())
        {
            var adminRole = new Role { Name = "超级管理员", Description = "拥有所有权限", CreatedAt = DateTime.Now };
            db.Roles.Add(adminRole);
            db.SaveChanges();
            logger.LogInformation("角色已初始化");
        }

        // 种子菜单
        if (!db.Menus.Any())
        {
            var menus = new List<Menu>
            {
                new() { Name = "仪表盘",    Path = "/dashboard",    Icon = "dashboard",     ParentId = 0, Sort = 1, CreatedAt = DateTime.Now },
                new() { Name = "核心优势",  Path = "/advantages",   Icon = "stars",         ParentId = 0, Sort = 2, CreatedAt = DateTime.Now },
                new() { Name = "技能管理",  Path = "/skills",       Icon = "code",          ParentId = 0, Sort = 3, CreatedAt = DateTime.Now },
                new() { Name = "项目管理",  Path = "/projects",     Icon = "deployed_code", ParentId = 0, Sort = 4, CreatedAt = DateTime.Now },
                new() { Name = "工作经历",  Path = "/work-history", Icon = "work",          ParentId = 0, Sort = 5, CreatedAt = DateTime.Now },
                new() { Name = "技能诊断",  Path = "/diagnostics",  Icon = "monitoring",    ParentId = 0, Sort = 6, CreatedAt = DateTime.Now },
                new() { Name = "系统管理",  Path = "",              Icon = "settings",      ParentId = 0, Sort = 99, CreatedAt = DateTime.Now },
                new() { Name = "用户管理",  Path = "/users",        Icon = "group",         ParentId = 7, Sort = 1, CreatedAt = DateTime.Now },
                new() { Name = "角色管理",  Path = "/roles",        Icon = "shield",        ParentId = 7, Sort = 2, CreatedAt = DateTime.Now },
                new() { Name = "菜单管理",  Path = "/menus",        Icon = "menu",          ParentId = 7, Sort = 3, CreatedAt = DateTime.Now },
            };
            db.Menus.AddRange(menus);
            db.SaveChanges();
            logger.LogInformation("菜单已初始化");
        }

        // 种子 RoleMenus：超级管理员拥有所有菜单
        var adminRoleId = db.Roles.First().Id;
        if (!db.RoleMenus.Any())
        {
            var allMenuIds = db.Menus.Select(m => m.Id).ToList();
            db.RoleMenus.AddRange(allMenuIds.Select(menuId => new RoleMenu { RoleId = adminRoleId, MenuId = menuId }));
            db.SaveChanges();
            logger.LogInformation("角色菜单权限已初始化");
        }

        // 种子管理员账户
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = PasswordHelper.Hash("admin123"),
                RoleId = adminRoleId,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
            logger.LogInformation("管理员账户已创建: admin / admin123");
        }
        else
        {
            // 确保已有用户关联角色（容错：用户手动建表时可能没有RoleId）
            var usersWithoutRole = await db.Users.Where(u => u.RoleId == 0).ToListAsync();
            foreach (var u in usersWithoutRole)
            {
                u.RoleId = adminRoleId;
            }
            if (usersWithoutRole.Count > 0)
            {
                await db.SaveChangesAsync();
                logger.LogInformation("已为 {Count} 个用户补全角色", usersWithoutRole.Count);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "数据库初始化遇到问题（可能数据库尚未就绪）");
    }
}

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
