using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Identity;

namespace General.Admin.Infrastructure;

public class JwtTokenService : ITransientDependency
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string CreateAccessToken(IdentityUser user, IReadOnlyCollection<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(AbpClaimTypes.UserId, user.Id.ToString()),
            new(AbpClaimTypes.UserName, user.UserName ?? string.Empty),
            new(AbpClaimTypes.Name, string.IsNullOrWhiteSpace(user.Name) ? user.UserName ?? string.Empty : user.Name),
            new(AbpClaimTypes.SurName, user.Surname ?? string.Empty)
        };

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AbpClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
