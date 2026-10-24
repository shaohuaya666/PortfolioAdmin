using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PortfolioAdmin.Api.Middleware;

public class JwtHelper : IJwtService
{
    /// <summary>
    /// 签发方默认值。签发（本类）与校验（Program.cs）必须共用同一来源，
    /// 否则会出现"签发用 A、校验用 B"导致全站 401 且难以排查。
    /// </summary>
    public const string DefaultIssuer = "PortfolioAdmin";
    public const string DefaultAudience = "PortfolioAdmin";

    private readonly IConfiguration _config;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
        _issuer = ResolveIssuer(config);
        _audience = ResolveAudience(config);
    }

    /// <summary>
    /// 解析签发方。用 <c>IsNullOrWhiteSpace</c> 而非 <c>??</c>：
    /// docker compose 在变量未定义时会注入空字符串，<c>??</c> 接不住空串。
    /// </summary>
    public static string ResolveIssuer(IConfiguration config)
        => string.IsNullOrWhiteSpace(config["Jwt:Issuer"]) ? DefaultIssuer : config["Jwt:Issuer"]!;

    /// <inheritdoc cref="ResolveIssuer" />
    public static string ResolveAudience(IConfiguration config)
        => string.IsNullOrWhiteSpace(config["Jwt:Audience"]) ? DefaultAudience : config["Jwt:Audience"]!;

    public string GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "480");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
