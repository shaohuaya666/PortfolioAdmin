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
public class WorkHistoriesController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public WorkHistoriesController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<WorkHistory>> GetAll()
        => await _db.WorkHistories.Include(w => w.Achievements).OrderBy(w => w.IsCurrent ? 0 : 1).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkHistory>> GetById(string id)
    {
        var entity = await _db.WorkHistories.Include(w => w.Achievements).FirstOrDefaultAsync(w => w.Id == id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<WorkHistory>> Create(WorkHistoryRequest dto)
    {
        if (await _db.WorkHistories.AnyAsync(w => w.Id == dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = new WorkHistory
        {
            Id = dto.Id,
            Company = dto.Company,
            Role = dto.Role,
            Period = dto.Period,
            Desc = dto.Desc,
            IsCurrent = dto.IsCurrent
        };
        _db.WorkHistories.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, WorkHistoryRequest dto)
    {
        var entity = await _db.WorkHistories.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Company = dto.Company;
        entity.Role = dto.Role;
        entity.Period = dto.Period;
        entity.Desc = dto.Desc;
        entity.IsCurrent = dto.IsCurrent;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var entity = await _db.WorkHistories.FindAsync(id);
        if (entity is null) return NotFound();
        _db.WorkHistories.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
