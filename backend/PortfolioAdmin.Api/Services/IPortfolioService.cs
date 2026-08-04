using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Services;

public interface IPortfolioService
{
    // ============ Advantages ============
    Task<List<Advantage>> GetAdvantagesAsync();
    Task<Advantage?> GetAdvantageByIdAsync(string id);
    Task<bool> AdvantageExistsAsync(string id);
    Task<Advantage> CreateAdvantageAsync(Advantage entity);
    Task<Advantage?> UpdateAdvantageAsync(string id, AdvantageRequest dto);
    Task<bool> DeleteAdvantageAsync(string id);

    // ============ SkillCategories ============
    Task<List<SkillCategory>> GetSkillCategoriesAsync();
    Task<SkillCategory?> GetSkillCategoryByIdAsync(int id);
    Task<SkillCategory> CreateSkillCategoryAsync(SkillCategory entity);
    Task<SkillCategory?> UpdateSkillCategoryAsync(int id, SkillCategoryRequest dto);
    Task<bool> DeleteSkillCategoryAsync(int id);

    // ============ Tags ============
    Task<List<TagInfo>> GetTagsAsync(int? categoryId = null);
    Task<TagInfo?> GetTagByIdAsync(int id);
    Task<bool> SkillCategoryExistsAsync(int id);
    Task<TagInfo> CreateTagAsync(TagInfo entity);
    Task<TagInfo?> UpdateTagAsync(int id, TagInfoRequest dto);
    Task<bool> DeleteTagAsync(int id);

    // ============ SkillDiagnostics ============
    Task<List<SkillDiagnostic>> GetSkillDiagnosticsAsync();
    Task<SkillDiagnostic?> GetSkillDiagnosticByIdAsync(int id);
    Task<SkillDiagnostic> CreateSkillDiagnosticAsync(SkillDiagnostic entity);
    Task<SkillDiagnostic?> UpdateSkillDiagnosticAsync(int id, SkillDiagnosticRequest dto);
    Task<bool> DeleteSkillDiagnosticAsync(int id);

    // ============ WorkHistories ============
    Task<List<WorkHistory>> GetWorkHistoriesAsync();
    Task<WorkHistory?> GetWorkHistoryByIdAsync(string id);
    Task<bool> WorkHistoryExistsAsync(string id);
    Task<WorkHistory> CreateWorkHistoryAsync(WorkHistory entity);
    Task<WorkHistory?> UpdateWorkHistoryAsync(string id, WorkHistoryRequest dto);
    Task<bool> DeleteWorkHistoryAsync(string id);

    // ============ Achievements ============
    Task<List<Achievement>> GetAchievementsAsync(string? workHistoryId = null);
    Task<Achievement?> GetAchievementByIdAsync(int id);
    Task<bool> WorkHistoryExistsForAchievementAsync(string workHistoryId);
    Task<Achievement> CreateAchievementAsync(Achievement entity);
    Task<Achievement?> UpdateAchievementAsync(int id, AchievementRequest dto);
    Task<bool> DeleteAchievementAsync(int id);

    // ============ CompactProjects ============
    Task<List<CompactProject>> GetCompactProjectsAsync();
    Task<CompactProject?> GetCompactProjectByIdAsync(string id);
    Task<bool> CompactProjectExistsAsync(string id);
    Task<CompactProject> CreateCompactProjectAsync(CompactProject entity);
    Task<CompactProject?> UpdateCompactProjectAsync(string id, CompactProjectRequest dto);
    Task<bool> DeleteCompactProjectAsync(string id);

    // ============ ProjectSkills ============
    Task<List<ProjectSkill>> GetProjectSkillsAsync(string? projectId = null);
    Task<ProjectSkill?> GetProjectSkillByIdAsync(int id);
    Task<bool> CompactProjectExistsForSkillAsync(string projectId);
    Task<ProjectSkill> CreateProjectSkillAsync(ProjectSkill entity);
    Task<ProjectSkill?> UpdateProjectSkillAsync(int id, ProjectSkillRequest dto);
    Task<bool> DeleteProjectSkillAsync(int id);
}
