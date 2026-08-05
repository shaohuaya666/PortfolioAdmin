using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.Attributes;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectSkillsController : ControllerBase
{
    private readonly IPortfolioService _service;

    public ProjectSkillsController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<ProjectSkill>> GetAll([FromQuery] string? projectId)
        => await _service.GetProjectSkillsAsync(projectId);

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectSkill>> GetById(int id)
    {
        var entity = await _service.GetProjectSkillByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    [RequirePermission("projects:skills_create")]
    public async Task<ActionResult<ProjectSkill>> Create(ProjectSkillRequest dto)
    {
        if (!await _service.CompactProjectExistsForSkillAsync(dto.ProjectId))
            return BadRequest(new { message = "所属项目不存在" });

        var entity = await _service.CreateProjectSkillAsync(new ProjectSkill
        {
            Name = dto.Name, ProjectId = dto.ProjectId
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    [RequirePermission("projects:skills_edit")]
    public async Task<IActionResult> Update(int id, ProjectSkillRequest dto)
    {
        var entity = await _service.UpdateProjectSkillAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    [RequirePermission("projects:skills_delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteProjectSkillAsync(id);
        return success ? NoContent() : NotFound();
    }
}
