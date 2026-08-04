using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) { }

    public DbSet<Advantage> Advantages => Set<Advantage>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<TagInfo> Tags => Set<TagInfo>();
    public DbSet<WorkHistory> WorkHistories => Set<WorkHistory>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<CompactProject> CompactProjects => Set<CompactProject>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<SkillDiagnostic> SkillDiagnostics => Set<SkillDiagnostic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 反射扫描所有 DbSet 属性，自动注册实体并应用 DataAnnotation 配置
        AutoConfigureEntities(modelBuilder);

        // FK 级联关系仍然需要通过反射自动配置
        AutoConfigureRelationships(modelBuilder);
    }

    /// <summary>
    /// 通过反射扫描所有实体类型，自动绑定表名和属性约束
    /// </summary>
    private void AutoConfigureEntities(ModelBuilder modelBuilder)
    {
        var entityTypes = this.GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType
                     && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();

        foreach (var entityType in entityTypes)
        {
            var entityMethod = typeof(ModelBuilder).GetMethod(nameof(ModelBuilder.Entity), Type.EmptyTypes);
            if (entityMethod == null) continue;

            var genericMethod = entityMethod.MakeGenericMethod(entityType);
            var entityBuilder = genericMethod.Invoke(modelBuilder, null);

            // 通过反射调用 entityBuilder.ToTable()、.Property().HasMaxLength() 等
            // 实际上 EF Core 已经会根据 [Table]、[MaxLength] 等 DataAnnotation 自动配置
            // 这里反射调用的意义在于：无论有多少实体，OnModelCreating 都不需要逐一手动配置
            // 所有配置都由 DataAnnotation 驱动，反射确保每个实体类型都被注册
            // 表名由 [Table] 注解决定，MaxLength 由 [MaxLength] 决定
        }
    }

    /// <summary>
    /// 反射扫描所有导航属性，自动配置 FK 级联删除关系
    /// </summary>
    private void AutoConfigureRelationships(ModelBuilder modelBuilder)
    {
        // 获取 Models 命名空间下所有实体类型（通过反射扫描）
        var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "PortfolioAdmin.Api.Models")
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // 查找所有 ICollection<> 导航属性，自动配置级联删除
            foreach (var prop in entityType.GetProperties())
            {
                if (!prop.PropertyType.IsGenericType) continue;

                var genericDef = prop.PropertyType.GetGenericTypeDefinition();
                if (genericDef != typeof(ICollection<>) && genericDef != typeof(List<>)) continue;

                var childType = prop.PropertyType.GetGenericArguments()[0];

                // 通过反射调用 modelBuilder.Entity<entityType>().HasMany(prop.Name).WithOne(...).HasForeignKey(...).OnDelete(Cascade)
                var entityBuilder = modelBuilder.Entity(entityType);
                var hasMany = entityBuilder.GetType().GetMethod("HasMany", new[] { typeof(string) });
                if (hasMany == null) continue;

                var collectionBuilder = hasMany.Invoke(entityBuilder, new object[] { prop.Name });
                if (collectionBuilder == null) continue;

                // .WithOne() - 不带参数的版本
                var withOne = collectionBuilder.GetType().GetMethod("WithOne", Type.EmptyTypes);
                var referenceBuilder = withOne?.Invoke(collectionBuilder, null);
                if (referenceBuilder == null) continue;

                // .HasForeignKey("FK属性名") - 需要在 childType 上找指向 entityType 的 FK
                var fkProp = childType.GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null
                                      && p.PropertyType == entityType);
                if (fkProp != null)
                {
                    // 找到对应的 FK id 属性
                    var fkAttr = fkProp.GetCustomAttribute<ForeignKeyAttribute>();
                    var fkPropName = fkAttr?.Name;
                    if (string.IsNullOrEmpty(fkPropName))
                    {
                        // 尝试按约定：NavigationPropertyName + "Id"
                        var childNavProps = childType.GetProperties()
                            .Where(p => p.PropertyType == entityType).ToList();
                        fkPropName = childNavProps.Count == 1 ? childNavProps[0].Name + "Id" : null;
                    }

                    if (!string.IsNullOrEmpty(fkPropName))
                    {
                        var hasFk = referenceBuilder.GetType().GetMethod("HasForeignKey", new[] { typeof(string) });
                        var builder = hasFk?.Invoke(referenceBuilder, new object[] { fkPropName });
                        if (builder != null)
                        {
                            var onDelete = builder.GetType().GetMethod("OnDelete", new[] { typeof(DeleteBehavior) });
                            onDelete?.Invoke(builder, new object[] { DeleteBehavior.Cascade });
                        }
                    }
                }
            }
        }
    }
}
