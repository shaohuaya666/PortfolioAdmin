using Microsoft.AspNetCore.Mvc;
using PortfolioAdmin.Api.DTOs;
using PortfolioAdmin.Api.Services;

namespace PortfolioAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captcha;

    public CaptchaController(ICaptchaService captcha) => _captcha = captcha;

    [HttpGet("generate")]
    public ActionResult<CaptchaData> Generate()
    {
        return Ok(_captcha.Generate());
    }

    [HttpPost("verify")]
    public IActionResult Verify([FromBody] CaptchaVerifyRequest request)
    {
        var (success, message) = _captcha.Verify(request);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}
