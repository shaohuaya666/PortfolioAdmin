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
public class TagsController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public TagsController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<TagInfo>> GetAll([FromQuery] int? categoryId)
    {
        var query = _db.Tags.AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(t => t.SkillCategoryId == categoryId.Value);
        return await query.OrderBy(t => t.Id).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagInfo>> GetById(int id)
    {
        var entity = await _db.Tags.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<TagInfo>> Create(TagInfoRequest dto)
    {
        if (!await _db.SkillCategories.AnyAsync(c => c.Id == dto.SkillCategoryId))
            return BadRequest(new { message = "所属分类不存在" });

        var entity = new TagInfo
        {
            Name = dto.Name,
            IsCore = dto.IsCore,
            SkillCategoryId = dto.SkillCategoryId
        };
        _db.Tags.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TagInfoRequest dto)
    {
        var entity = await _db.Tags.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Name = dto.Name;
        entity.IsCore = dto.IsCore;
        entity.SkillCategoryId = dto.SkillCategoryId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Tags.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Tags.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
