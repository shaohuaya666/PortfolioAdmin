using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioAdmin.Api.Models;

public class Advantage
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Num { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
}

public class SkillCategory
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = "primary";
    public List<TagInfo> Tags { get; set; } = new();
}

public class TagInfo
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public int SkillCategoryId { get; set; }

    [ForeignKey(nameof(SkillCategoryId))]
    public SkillCategory? SkillCategory { get; set; }
}

public class WorkHistory
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public List<Achievement> Achievements { get; set; } = new();
}

public class Achievement
{
    [Key]
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string WorkHistoryId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkHistoryId))]
    public WorkHistory? WorkHistory { get; set; }
}

public class CompactProject
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public List<ProjectSkill> Skills { get; set; } = new();
}

public class ProjectSkill
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProjectId))]
    public CompactProject? Project { get; set; }
}

public class SkillDiagnostic
{
    [Key]
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string Stat { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
