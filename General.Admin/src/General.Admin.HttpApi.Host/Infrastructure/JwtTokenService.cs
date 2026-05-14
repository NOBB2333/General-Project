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

    public string CreateAccessToken(
        IdentityUser user,
        IReadOnlyCollection<string> roles,
        Guid? tenantIdOverride = null,
        bool isHostTenantOperation = false,
        Guid? operationTenantId = null,
        string? operationTenantName = null,
        Guid? hostOperatorUserId = null,
        string? hostOperatorUserName = null,
        string? operationSessionId = null)
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
        var tenantId = tenantIdOverride ?? user.TenantId;
        if (tenantId.HasValue)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.Value.ToString()));
        }

        if (isHostTenantOperation)
        {
            claims.Add(new Claim(PlatformTenantOperationClaimTypes.HostTenantOperation, "true"));
            claims.Add(new Claim(PlatformTenantOperationClaimTypes.OriginalTenantId, user.TenantId?.ToString() ?? string.Empty));
            claims.Add(new Claim(PlatformTenantOperationClaimTypes.ImpersonatedUserId, user.Id.ToString()));
            claims.Add(new Claim(PlatformTenantOperationClaimTypes.ImpersonatedUserName, user.UserName ?? string.Empty));
            if (operationTenantId.HasValue)
            {
                claims.Add(new Claim(PlatformTenantOperationClaimTypes.OperationTenantId, operationTenantId.Value.ToString()));
            }
            if (hostOperatorUserId.HasValue)
            {
                claims.Add(new Claim(PlatformTenantOperationClaimTypes.HostOperatorUserId, hostOperatorUserId.Value.ToString()));
            }
            if (!string.IsNullOrWhiteSpace(hostOperatorUserName))
            {
                claims.Add(new Claim(PlatformTenantOperationClaimTypes.HostOperatorUserName, hostOperatorUserName));
            }
            if (!string.IsNullOrWhiteSpace(operationSessionId))
            {
                claims.Add(new Claim(PlatformTenantOperationClaimTypes.OperationSessionId, operationSessionId));
            }
            if (!string.IsNullOrWhiteSpace(operationTenantName))
            {
                claims.Add(new Claim(PlatformTenantOperationClaimTypes.OperationTenantName, operationTenantName));
            }
        }

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
