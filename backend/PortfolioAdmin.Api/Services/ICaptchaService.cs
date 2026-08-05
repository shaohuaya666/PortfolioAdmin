using PortfolioAdmin.Api.DTOs;

namespace PortfolioAdmin.Api.Services;

public interface ICaptchaService
{
    CaptchaData Generate();
    (bool Success, string Message) Verify(CaptchaVerifyRequest request);
    bool IsVerified(string captchaId);
    void Consume(string captchaId);
}
