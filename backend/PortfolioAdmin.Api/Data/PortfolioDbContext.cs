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

        modelBuilder.Entity<Advantage>(entity =>
        {
            entity.ToTable("Advantages");
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Num).HasMaxLength(10);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Desc).HasMaxLength(500);
        });

        modelBuilder.Entity<SkillCategory>(entity =>
        {
            entity.ToTable("SkillCategories");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.ThemeColor).HasMaxLength(50);
            entity.HasMany(e => e.Tags)
                .WithOne(t => t.SkillCategory)
                .HasForeignKey(t => t.SkillCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TagInfo>(entity =>
        {
            entity.ToTable("Tags");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<WorkHistory>(entity =>
        {
            entity.ToTable("WorkHistories");
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(200);
            entity.Property(e => e.Period).HasMaxLength(50);
            entity.Property(e => e.Desc).HasMaxLength(500);
            entity.HasMany(e => e.Achievements)
                .WithOne(a => a.WorkHistory)
                .HasForeignKey(a => a.WorkHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.ToTable("Achievements");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.WorkHistoryId).HasMaxLength(50);
        });

        modelBuilder.Entity<CompactProject>(entity =>
        {
            entity.ToTable("CompactProjects");
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.Year).HasMaxLength(50);
            entity.Property(e => e.Desc).HasMaxLength(500);
            entity.HasMany(e => e.Skills)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.ToTable("ProjectSkills");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ProjectId).HasMaxLength(50);
        });

        modelBuilder.Entity<SkillDiagnostic>(entity =>
        {
            entity.ToTable("SkillDiagnostics");
            entity.Property(e => e.TagName).HasMaxLength(100);
            entity.Property(e => e.Desc).HasMaxLength(500);
            entity.Property(e => e.Stat).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.ToTable("Menus");
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Path).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(100);
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.ToTable("RoleMenus");
            entity.HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
