using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Models;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IPortfolioService _service;

    public TagsController(IPortfolioService service) => _service = service;

    [HttpGet]
    public async Task<List<TagInfo>> GetAll([FromQuery] int? categoryId)
        => await _service.GetTagsAsync(categoryId);

    [HttpGet("{id}")]
    public async Task<ActionResult<TagInfo>> GetById(int id)
    {
        var entity = await _service.GetTagByIdAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPost]
    public async Task<ActionResult<TagInfo>> Create(TagInfoRequest dto)
    {
        if (!await _service.SkillCategoryExistsAsync(dto.SkillCategoryId))
            return BadRequest(new { message = "所属分类不存在" });

        var entity = await _service.CreateTagAsync(new TagInfo
        {
            Name = dto.Name, IsCore = dto.IsCore, SkillCategoryId = dto.SkillCategoryId
        });
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TagInfoRequest dto)
    {
        var entity = await _service.UpdateTagAsync(id, dto);
        return entity is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteTagAsync(id);
        return success ? NoContent() : NotFound();
    }
}
