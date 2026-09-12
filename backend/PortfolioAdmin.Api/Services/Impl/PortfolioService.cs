using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Services;

public class PortfolioService : IPortfolioService
{
    private readonly PortfolioDbContext _db;

    public PortfolioService(PortfolioDbContext db)
    {
        _db = db;
    }

    // ============ Advantages ============
    public async Task<List<Advantage>> GetAdvantagesAsync()
    {
        return await _db.Advantages.OrderBy(a => a.Num).AsNoTracking().ToListAsync();
    }

    public async Task<Advantage?> GetAdvantageByIdAsync(string id)
    {
        return await _db.Advantages.FindAsync(id);
    }

    public async Task<bool> AdvantageExistsAsync(string id)
    {
        return await _db.Advantages.AnyAsync(a => a.Id == id);
    }

    public async Task<Advantage> CreateAdvantageAsync(Advantage entity)
    {
        _db.Advantages.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Advantage?> UpdateAdvantageAsync(string id, AdvantageRequest dto)
    {
        var entity = await _db.Advantages.FindAsync(id);
        if (entity == null) return null;

        if (dto.Id != id && await _db.Advantages.AnyAsync(a => a.Id == dto.Id))
            return null;

        entity.Id = dto.Id;
        entity.Num = dto.Num;
        entity.Title = dto.Title;
        entity.Desc = dto.Desc;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAdvantageAsync(string id)
    {
        var entity = await _db.Advantages.FindAsync(id);
        if (entity == null) return false;
        _db.Advantages.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ SkillCategories ============
    public async Task<List<SkillCategory>> GetSkillCategoriesAsync()
    {
        return await _db.SkillCategories
            .Include(s => s.Tags)
            .OrderBy(s => s.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SkillCategory?> GetSkillCategoryByIdAsync(int id)
    {
        return await _db.SkillCategories.FindAsync(id);
    }

    public async Task<SkillCategory> CreateSkillCategoryAsync(SkillCategory entity)
    {
        _db.SkillCategories.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<SkillCategory?> UpdateSkillCategoryAsync(int id, SkillCategoryRequest dto)
    {
        var entity = await _db.SkillCategories.FindAsync(id);
        if (entity == null) return null;

        entity.Title = dto.Title;
        entity.ThemeColor = dto.ThemeColor;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteSkillCategoryAsync(int id)
    {
        var entity = await _db.SkillCategories.FindAsync(id);
        if (entity == null) return false;
        _db.SkillCategories.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ Tags ============
    public async Task<List<TagInfo>> GetTagsAsync(int? categoryId = null)
    {
        var query = _db.Tags.Include(t => t.SkillCategory).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(t => t.SkillCategoryId == categoryId.Value);
        return await query.OrderBy(t => t.Id).AsNoTracking().ToListAsync();
    }

    public async Task<TagInfo?> GetTagByIdAsync(int id)
    {
        return await _db.Tags.FindAsync(id);
    }

    public async Task<bool> SkillCategoryExistsAsync(int id)
    {
        return await _db.SkillCategories.AnyAsync(s => s.Id == id);
    }

    public async Task<TagInfo> CreateTagAsync(TagInfo entity)
    {
        _db.Tags.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<TagInfo?> UpdateTagAsync(int id, TagInfoRequest dto)
    {
        var entity = await _db.Tags.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.IsCore = dto.IsCore;
        entity.SkillCategoryId = dto.SkillCategoryId;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteTagAsync(int id)
    {
        var entity = await _db.Tags.FindAsync(id);
        if (entity == null) return false;
        _db.Tags.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ SkillDiagnostics ============
    public async Task<List<SkillDiagnostic>> GetSkillDiagnosticsAsync()
    {
        return await _db.SkillDiagnostics.OrderBy(s => s.Id).AsNoTracking().ToListAsync();
    }

    public async Task<SkillDiagnostic?> GetSkillDiagnosticByIdAsync(int id)
    {
        return await _db.SkillDiagnostics.FindAsync(id);
    }

    public async Task<SkillDiagnostic> CreateSkillDiagnosticAsync(SkillDiagnostic entity)
    {
        _db.SkillDiagnostics.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<SkillDiagnostic?> UpdateSkillDiagnosticAsync(int id, SkillDiagnosticRequest dto)
    {
        var entity = await _db.SkillDiagnostics.FindAsync(id);
        if (entity == null) return null;

        entity.TagName = dto.TagName;
        entity.Desc = dto.Desc;
        entity.Stat = dto.Stat;
        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteSkillDiagnosticAsync(int id)
    {
        var entity = await _db.SkillDiagnostics.FindAsync(id);
        if (entity == null) return false;
        _db.SkillDiagnostics.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ WorkHistories ============
    public async Task<List<WorkHistory>> GetWorkHistoriesAsync()
    {
        return await _db.WorkHistories
            .Include(w => w.Achievements)
            .OrderByDescending(w => w.IsCurrent)
            .ThenByDescending(w => w.Period)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<WorkHistory?> GetWorkHistoryByIdAsync(string id)
    {
        return await _db.WorkHistories.FindAsync(id);
    }

    public async Task<bool> WorkHistoryExistsAsync(string id)
    {
        return await _db.WorkHistories.AnyAsync(w => w.Id == id);
    }

    public async Task<WorkHistory> CreateWorkHistoryAsync(WorkHistory entity)
    {
        _db.WorkHistories.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<WorkHistory?> UpdateWorkHistoryAsync(string id, WorkHistoryRequest dto)
    {
        var entity = await _db.WorkHistories.FindAsync(id);
        if (entity == null) return null;

        if (dto.Id != id && await _db.WorkHistories.AnyAsync(w => w.Id == dto.Id))
            return null;

        entity.Id = dto.Id;
        entity.Company = dto.Company;
        entity.Role = dto.Role;
        entity.Period = dto.Period;
        entity.Desc = dto.Desc;
        entity.IsCurrent = dto.IsCurrent;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteWorkHistoryAsync(string id)
    {
        var entity = await _db.WorkHistories.FindAsync(id);
        if (entity == null) return false;
        _db.WorkHistories.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ Achievements ============
    public async Task<List<Achievement>> GetAchievementsAsync(string? workHistoryId = null)
    {
        var query = _db.Achievements.AsQueryable();
        if (!string.IsNullOrEmpty(workHistoryId))
            query = query.Where(a => a.WorkHistoryId == workHistoryId);
        return await query.OrderBy(a => a.Id).AsNoTracking().ToListAsync();
    }

    public async Task<Achievement?> GetAchievementByIdAsync(int id)
    {
        return await _db.Achievements.FindAsync(id);
    }

    public async Task<bool> WorkHistoryExistsForAchievementAsync(string workHistoryId)
    {
        return await _db.WorkHistories.AnyAsync(w => w.Id == workHistoryId);
    }

    public async Task<Achievement> CreateAchievementAsync(Achievement entity)
    {
        _db.Achievements.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<Achievement?> UpdateAchievementAsync(int id, AchievementRequest dto)
    {
        var entity = await _db.Achievements.FindAsync(id);
        if (entity == null) return null;

        entity.Description = dto.Description;
        entity.WorkHistoryId = dto.WorkHistoryId;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAchievementAsync(int id)
    {
        var entity = await _db.Achievements.FindAsync(id);
        if (entity == null) return false;
        _db.Achievements.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ CompactProjects ============
    public async Task<List<CompactProject>> GetCompactProjectsAsync()
    {
        return await _db.CompactProjects
            .Include(p => p.Skills)
            .OrderBy(p => p.Year)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CompactProject?> GetCompactProjectByIdAsync(string id)
    {
        return await _db.CompactProjects.FindAsync(id);
    }

    public async Task<bool> CompactProjectExistsAsync(string id)
    {
        return await _db.CompactProjects.AnyAsync(p => p.Id == id);
    }

    public async Task<CompactProject> CreateCompactProjectAsync(CompactProject entity)
    {
        _db.CompactProjects.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<CompactProject?> UpdateCompactProjectAsync(string id, CompactProjectRequest dto)
    {
        var entity = await _db.CompactProjects.FindAsync(id);
        if (entity == null) return null;

        if (dto.Id != id && await _db.CompactProjects.AnyAsync(p => p.Id == dto.Id))
            return null;

        entity.Id = dto.Id;
        entity.Title = dto.Title;
        entity.Type = dto.Type;
        entity.Year = dto.Year;
        entity.Desc = dto.Desc;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteCompactProjectAsync(string id)
    {
        var entity = await _db.CompactProjects.FindAsync(id);
        if (entity == null) return false;
        _db.CompactProjects.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ============ ProjectSkills ============
    public async Task<List<ProjectSkill>> GetProjectSkillsAsync(string? projectId = null)
    {
        var query = _db.ProjectSkills.AsQueryable();
        if (!string.IsNullOrEmpty(projectId))
            query = query.Where(s => s.ProjectId == projectId);
        return await query.OrderBy(s => s.Id).AsNoTracking().ToListAsync();
    }

    public async Task<ProjectSkill?> GetProjectSkillByIdAsync(int id)
    {
        return await _db.ProjectSkills.FindAsync(id);
    }

    public async Task<bool> CompactProjectExistsForSkillAsync(string projectId)
    {
        return await _db.CompactProjects.AnyAsync(p => p.Id == projectId);
    }

    public async Task<ProjectSkill> CreateProjectSkillAsync(ProjectSkill entity)
    {
        _db.ProjectSkills.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<ProjectSkill?> UpdateProjectSkillAsync(int id, ProjectSkillRequest dto)
    {
        var entity = await _db.ProjectSkills.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.ProjectId = dto.ProjectId;
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteProjectSkillAsync(int id)
    {
        var entity = await _db.ProjectSkills.FindAsync(id);
        if (entity == null) return false;
        _db.ProjectSkills.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
