using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillDiagnosticsController : ControllerBase
{
    private readonly IPortfolioService _service;

    public SkillDiagnosticsController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<SkillDiagnostic>> GetAll()
        => await _service.GetSkillDiagnosticsAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SkillDiagnostic>> GetById(int id)
    {
        var entity = await _service.GetSkillDiagnosticByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<SkillDiagnostic>> Create(SkillDiagnosticRequest dto)
    {
        var entity = await _service.CreateSkillDiagnosticAsync(new SkillDiagnostic
        {
            TagName = dto.TagName, Desc = dto.Desc, Stat = dto.Stat, Status = dto.Status
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SkillDiagnosticRequest dto)
    {
        var entity = await _service.UpdateSkillDiagnosticAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteSkillDiagnosticAsync(id);
        return success ? NoContent() : NotFound();
    }
}
