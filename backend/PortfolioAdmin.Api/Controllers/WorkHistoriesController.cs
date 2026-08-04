using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkHistoriesController : ControllerBase
{
    private readonly IPortfolioService _service;

    public WorkHistoriesController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<WorkHistory>> GetAll()
        => await _service.GetWorkHistoriesAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkHistory>> GetById(string id)
    {
        var entity = await _service.GetWorkHistoryByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<WorkHistory>> Create(WorkHistoryRequest dto)
    {
        if (await _service.WorkHistoryExistsAsync(dto.Id))
            return Conflict(new { message = "ID 已存在" });

        var entity = await _service.CreateWorkHistoryAsync(new WorkHistory
        {
            Id = dto.Id, Company = dto.Company, Role = dto.Role,
            Period = dto.Period, Desc = dto.Desc, IsCurrent = dto.IsCurrent
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, WorkHistoryRequest dto)
    {
        var entity = await _service.UpdateWorkHistoryAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _service.DeleteWorkHistoryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
