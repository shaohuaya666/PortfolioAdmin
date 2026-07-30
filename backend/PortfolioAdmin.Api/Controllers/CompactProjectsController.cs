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
public class CompactProjectsController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public CompactProjectsController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<CompactProject>> GetAll()
        => await _db.CompactProjects.Include(p => p.Skills).OrderBy(p => p.Title).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<CompactProject>> GetById(string id)
    {
        var entity = await _db.CompactProjects.Include(p => p.Skills).FirstOrDefaultAsync(p => p.Id == id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<CompactProject>> Create(CompactProjectRequest dto)
    {
        if (await _db.CompactProjects.AnyAsync(p => p.Id == dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = new CompactProject
        {
            Id = dto.Id,
            Title = dto.Title,
            Type = dto.Type,
            Year = dto.Year,
            Desc = dto.Desc
        };
        _db.CompactProjects.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, CompactProjectRequest dto)
    {
        var entity = await _db.CompactProjects.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Title = dto.Title;
        entity.Type = dto.Type;
        entity.Year = dto.Year;
        entity.Desc = dto.Desc;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var entity = await _db.CompactProjects.FindAsync(id);
        if (entity is null) return NotFound();
        _db.CompactProjects.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
