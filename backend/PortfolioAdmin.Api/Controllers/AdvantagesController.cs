using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvantagesController : ControllerBase
{
    private readonly IPortfolioService _service;

    public AdvantagesController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<Advantage>> GetAll()
        => await _service.GetAdvantagesAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Advantage>> GetById(string id)
    {
        var entity = await _service.GetAdvantageByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<Advantage>> Create(AdvantageRequest dto)
    {
        if (await _service.AdvantageExistsAsync(dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = await _service.CreateAdvantageAsync(new Advantage
        {
            Id = dto.Id, Num = dto.Num, Title = dto.Title, Desc = dto.Desc
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AdvantageRequest dto)
    {
        var entity = await _service.UpdateAdvantageAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteAdvantageAsync(id);
        return success ? NoContent() : NotFound();
    }
}
