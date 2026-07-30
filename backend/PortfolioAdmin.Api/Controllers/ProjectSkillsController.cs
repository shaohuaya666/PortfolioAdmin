using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectSkillsController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public ProjectSkillsController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<ProjectSkill>> GetAll([FromQuery] string? projectId)
    {
        var query = _db.ProjectSkills.AsQueryable();
        if (!string.IsNullOrEmpty(projectId))
            query = query.Where(s => s.ProjectId == projectId);
        return await query.OrderBy(s => s.Id).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectSkill>> GetById(int id)
    {
        var entity = await _db.ProjectSkills.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<ProjectSkill>> Create(ProjectSkillRequest dto)
    {
        if (!await _db.CompactProjects.AnyAsync(p => p.Id == dto.ProjectId))
            return BadRequest(new { message = "所属项目不存在" });

        var entity = new ProjectSkill
        {
            Name = dto.Name,
            ProjectId = dto.ProjectId
        };
        _db.ProjectSkills.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProjectSkillRequest dto)
    {
        var entity = await _db.ProjectSkills.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Name = dto.Name;
        entity.ProjectId = dto.ProjectId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.ProjectSkills.FindAsync(id);
        if (entity is null) return NotFound();
        _db.ProjectSkills.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
