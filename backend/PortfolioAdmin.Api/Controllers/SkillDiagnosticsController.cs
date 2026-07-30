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
public class SkillDiagnosticsController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public SkillDiagnosticsController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<SkillDiagnostic>> GetAll()
        => await _db.SkillDiagnostics.OrderBy(d => d.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SkillDiagnostic>> GetById(int id)
    {
        var entity = await _db.SkillDiagnostics.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<SkillDiagnostic>> Create(SkillDiagnosticRequest dto)
    {
        var entity = new SkillDiagnostic
        {
            TagName = dto.TagName,
            Desc = dto.Desc,
            Stat = dto.Stat,
            Status = dto.Status
        };
        _db.SkillDiagnostics.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SkillDiagnosticRequest dto)
    {
        var entity = await _db.SkillDiagnostics.FindAsync(id);
        if (entity is null) return NotFound();

        entity.TagName = dto.TagName;
        entity.Desc = dto.Desc;
        entity.Stat = dto.Stat;
        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.SkillDiagnostics.FindAsync(id);
        if (entity is null) return NotFound();
        _db.SkillDiagnostics.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
