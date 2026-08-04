using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompactProjectsController : ControllerBase
{
    private readonly IPortfolioService _service;

    public CompactProjectsController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<CompactProject>> GetAll()
        => await _service.GetCompactProjectsAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<CompactProject>> GetById(string id)
    {
        var entity = await _service.GetCompactProjectByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<CompactProject>> Create(CompactProjectRequest dto)
    {
        if (await _service.CompactProjectExistsAsync(dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = await _service.CreateCompactProjectAsync(new CompactProject
        {
            Id = dto.Id, Title = dto.Title, Type = dto.Type, Year = dto.Year, Desc = dto.Desc
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, CompactProjectRequest dto)
    {
        var entity = await _service.UpdateCompactProjectAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteCompactProjectAsync(id);
        return success ? NoContent() : NotFound();
    }
}
