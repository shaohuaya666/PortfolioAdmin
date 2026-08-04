using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IPortfolioService _service;

    public AchievementsController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<Achievement>> GetAll([FromQuery] string? workHistoryId)
        => await _service.GetAchievementsAsync(workHistoryId);

    [HttpGet("{id}")]
    public async Task<ActionResult<Achievement>> GetById(int id)
    {
        var entity = await _service.GetAchievementByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<Achievement>> Create(AchievementRequest dto)
    {
        if (!await _service.WorkHistoryExistsForAchievementAsync(dto.WorkHistoryId))
            return BadRequest(new { message = "所属工作经历不存在" });

        var entity = await _service.CreateAchievementAsync(new Achievement
        {
            Description = dto.Description, WorkHistoryId = dto.WorkHistoryId
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AchievementRequest dto)
    {
        var entity = await _service.UpdateAchievementAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAchievementAsync(id);
        return success ? NoContent() : NotFound();
    }
}
