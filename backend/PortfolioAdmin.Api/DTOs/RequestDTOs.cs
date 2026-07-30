using System.ComponentModel.DataAnnotations;

namespace PortfolioAdmin.Api.DTOs;

// ========== Advantage ==========
public class AdvantageRequest
{
    [Required, MaxLength(50)]
    public string Id { get; set; } = string.Empty;
    [Required, MaxLength(10)]
    public string Num { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
}

// ========== SkillCategory ==========
public class SkillCategoryRequest
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(50)]
    public string ThemeColor { get; set; } = "primary";
}

// ========== TagInfo ==========
public class TagInfoRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public int SkillCategoryId { get; set; }
}

// ========== WorkHistory ==========
public class WorkHistoryRequest
{
    [Required, MaxLength(50)]
    public string Id { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Company { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Role { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Period { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

// ========== Achievement ==========
public class AchievementRequest
{
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    public string WorkHistoryId { get; set; } = string.Empty;
}

// ========== CompactProject ==========
public class CompactProjectRequest
{
    [Required, MaxLength(50)]
    public string Id { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Year { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
}

// ========== ProjectSkill ==========
public class ProjectSkillRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;
}

// ========== SkillDiagnostic ==========
public class SkillDiagnosticRequest
{
    [Required, MaxLength(100)]
    public string TagName { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Stat { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

// ========== Sort / Batch ==========
public class SortOrderRequest
{
    public List<SortItem> Items { get; set; } = new();
}

public class SortItem
{
    public string Id { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
