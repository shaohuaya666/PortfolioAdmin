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
public class AchievementsController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public AchievementsController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<Achievement>> GetAll([FromQuery] string? workHistoryId)
    {
        var query = _db.Achievements.AsQueryable();
        if (!string.IsNullOrEmpty(workHistoryId))
            query = query.Where(a => a.WorkHistoryId == workHistoryId);
        return await query.OrderBy(a => a.Id).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Achievement>> GetById(int id)
    {
        var entity = await _db.Achievements.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<Achievement>> Create(AchievementRequest dto)
    {
        if (!await _db.WorkHistories.AnyAsync(w => w.Id == dto.WorkHistoryId))
            return BadRequest(new { message = "所属工作经历不存在" });

        var entity = new Achievement
        {
            Description = dto.Description,
            WorkHistoryId = dto.WorkHistoryId
        };
        _db.Achievements.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AchievementRequest dto)
    {
        var entity = await _db.Achievements.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Description = dto.Description;
        entity.WorkHistoryId = dto.WorkHistoryId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Achievements.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Achievements.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
