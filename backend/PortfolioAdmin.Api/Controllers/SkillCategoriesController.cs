using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillCategoriesController : ControllerBase
{
    private readonly IPortfolioService _service;

    public SkillCategoriesController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<SkillCategory>> GetAll()
        => await _service.GetSkillCategoriesAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<SkillCategory>> GetById(int id)
    {
        var entity = await _service.GetSkillCategoryByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<SkillCategory>> Create(SkillCategoryRequest dto)
    {
        var entity = await _service.CreateSkillCategoryAsync(new SkillCategory
        {
            Title = dto.Title, ThemeColor = dto.ThemeColor
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SkillCategoryRequest dto)
    {
        var entity = await _service.UpdateSkillCategoryAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteSkillCategoryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
