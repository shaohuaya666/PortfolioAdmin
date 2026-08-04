using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioAdmin.Api.Models;

[Table("Advantages")]
public class Advantage
{
    [Key]
    public string Id { get; set; } = string.Empty;
    [MaxLength(10)]
    public string Num { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
}

[Table("SkillCategories")]
public class SkillCategory
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(50)]
    public string ThemeColor { get; set; } = "primary";
    public List<TagInfo> Tags { get; set; } = new();
}

[Table("Tags")]
public class TagInfo
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool IsCore { get; set; }
    public int SkillCategoryId { get; set; }

    [ForeignKey(nameof(SkillCategoryId))]
    public SkillCategory? SkillCategory { get; set; }
}

[Table("WorkHistories")]
public class WorkHistory
{
    [Key]
    public string Id { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Role { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Period { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public List<Achievement> Achievements { get; set; } = new();
}

[Table("Achievements")]
public class Achievement
{
    [Key]
    public int Id { get; set; }
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(50)]
    public string WorkHistoryId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkHistoryId))]
    public WorkHistory? WorkHistory { get; set; }
}

[Table("CompactProjects")]
public class CompactProject
{
    [Key]
    public string Id { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Year { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
    public List<ProjectSkill> Skills { get; set; } = new();
}

[Table("ProjectSkills")]
public class ProjectSkill
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string ProjectId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProjectId))]
    public CompactProject? Project { get; set; }
}

[Table("SkillDiagnostics")]
public class SkillDiagnostic
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    public string TagName { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Desc { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Stat { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
