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
public class AdvantagesController : ControllerBase
{
    private readonly PortfolioDbContext _db;

    public AdvantagesController(PortfolioDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<Advantage>> GetAll()
        => await _db.Advantages.OrderBy(a => a.Num).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Advantage>> GetById(string id)
    {
        var entity = await _db.Advantages.FindAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<Advantage>> Create(AdvantageRequest dto)
    {
        if (await _db.Advantages.AnyAsync(a => a.Id == dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = new Advantage
        {
            Id = dto.Id,
            Num = dto.Num,
            Title = dto.Title,
            Desc = dto.Desc
        };
        _db.Advantages.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AdvantageRequest dto)
    {
        var entity = await _db.Advantages.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Num = dto.Num;
        entity.Title = dto.Title;
        entity.Desc = dto.Desc;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var entity = await _db.Advantages.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Advantages.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
