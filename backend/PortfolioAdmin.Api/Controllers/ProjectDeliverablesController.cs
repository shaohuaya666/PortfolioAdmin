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
public class ProjectDeliverablesController : ControllerBase
{
    private readonly IPortfolioService _service;

    public ProjectDeliverablesController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<ProjectDeliverable>> GetAll([FromQuery] string? projectId, [FromQuery] string? status)
        => await _service.GetProjectDeliverablesAsync(projectId, status);

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDeliverable>> GetById(string id)
    {
        var entity = await _service.GetProjectDeliverableByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    [RequirePermission("deliverables:create")]
    public async Task<ActionResult<ProjectDeliverable>> Create(ProjectDeliverableRequest dto)
    {
        if (await _service.ProjectDeliverableExistsAsync(dto.Id))
            return Conflict(new { message = "ID 已存在" });

        if (!string.IsNullOrEmpty(dto.ProjectId) && !await _service.CompactProjectExistsForDeliverableAsync(dto.ProjectId))
            return BadRequest(new { message = "关联的项目不存在" });

        var entity = await _service.CreateProjectDeliverableAsync(new ProjectDeliverable
        {
            Id = dto.Id,
            Name = dto.Name,
            Version = dto.Version,
            Description = dto.Description,
            Status = dto.Status,
            Url = dto.Url,
            CoverImage = dto.CoverImage,
            TechTags = dto.TechTags,
            CompletedAt = dto.CompletedAt,
            Owner = dto.Owner,
            Remark = dto.Remark,
            SortOrder = dto.SortOrder,
            IsPublic = dto.IsPublic,
            ProjectId = string.IsNullOrEmpty(dto.ProjectId) ? null : dto.ProjectId
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    [RequirePermission("deliverables:edit")]
    public async Task<IActionResult> Update(string id, ProjectDeliverableRequest dto)
    {
        if (!string.IsNullOrEmpty(dto.ProjectId) && !await _service.CompactProjectExistsForDeliverableAsync(dto.ProjectId))
            return BadRequest(new { message = "关联的项目不存在" });

        var entity = await _service.UpdateProjectDeliverableAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    [RequirePermission("deliverables:delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteProjectDeliverableAsync(id);
        return success ? NoContent() : NotFound();
    }
}
