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
public class SkillCategoriesController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public SkillCategoriesController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<SkillCategory>> GetAll()
        => await _db.SkillCategories.Include(c => c.Tags).OrderBy(c => c.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SkillCategory>> GetById(int id)
    {
        var entity = await _db.SkillCategories.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<SkillCategory>> Create(SkillCategoryRequest dto)
    {
        var entity = new SkillCategory
        {
            Title = dto.Title,
            ThemeColor = dto.ThemeColor
        };
        _db.SkillCategories.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SkillCategoryRequest dto)
    {
        var entity = await _db.SkillCategories.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Title = dto.Title;
        entity.ThemeColor = dto.ThemeColor;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.SkillCategories.FindAsync(id);
        if (entity is null) return NotFound();
        _db.SkillCategories.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
