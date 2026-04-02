using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Platforms.Dtos;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Options;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Common.Services;

public class JwtTokenService(IOptions<JwtOptions> jwtOptions,
    ILogger<JwtTokenService> logger) : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateAccountAccessToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, user.PublicId.ToString()),
            new(JwtClaimNames.JwtVersion, user.JwtVersion.ToString()),
            new(JwtClaimNames.TokenType, JwtTokenType.AccountAccessToken.ToString()),
            new(JwtClaimNames.UserName, user.UserName!),
            new(JwtClaimNames.UserEmail, user.Email!)
        };

        return GenerateToken(
            claims,
            JwtTokenType.AccountAccessToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes));
    }

    public string GenerateTenantAccessToken(
        ApplicationUser user,
        TenantMembership membership)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, user.PublicId.ToString()),
            new(JwtClaimNames.JwtVersion, user.JwtVersion.ToString()),
            new(JwtClaimNames.Tenant, membership.Tenant.PublicId.ToString()),
            new(JwtClaimNames.Role, membership.Role.ToString()),
            new(JwtClaimNames.TokenType, JwtTokenType.TenantAccessToken.ToString()),
            new(JwtClaimNames.UserName, user.UserName!),
            new(JwtClaimNames.UserEmail, user.Email!),
            new(JwtClaimNames.TenantName, membership.Tenant.Name!),
        };

        return GenerateToken(
            claims,
            JwtTokenType.TenantAccessToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes));
    }

    public string GenerateRefreshToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, user.PublicId.ToString()),
            new(JwtClaimNames.JwtVersion, user.JwtVersion.ToString()),
            new(JwtClaimNames.TokenType, JwtTokenType.RefreshToken.ToString()),
            new(JwtClaimNames.UserName, user.UserName!),
            new(JwtClaimNames.UserEmail, user.Email!),
        };

        return GenerateToken(
            claims,
            JwtTokenType.RefreshToken,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));
    }

    public string GenerateInvitationToken(TenantInvitation invitation)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.InvitationPublicId, invitation.PublicId.ToString()),
            new(JwtClaimNames.InvitationVersion, invitation.Version.ToString()),
            new(JwtClaimNames.TokenType, JwtTokenType.InvitationToken.ToString())
        };

        return GenerateToken(
            claims,
            JwtTokenType.InvitationToken,
            invitation.ExpiredAt);
    }

    public Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token)
    {
        var (principal, resultCode) = GetPrincipalFromTokenInner(token);

        var result = principal == null
            ? ServiceResult<ClaimsPrincipal?>.Fail(
                resultCode,
                "Token could not be parsed.")
            : ServiceResult<ClaimsPrincipal?>.Ok(
                principal,
                resultCode,
                "Token parsed successfully.");

        return Task.FromResult(result);
    }

    public Task<ServiceResult<JwtParseDto>> ParseJwtTokenAsync(string token)
    {
        var (principalResult, resultCode) = GetPrincipalFromTokenInner(token);

        if (principalResult == null)
        {
            return Task.FromResult(ServiceResult<JwtParseDto>.Fail(
                resultCode,
                "Token could not be parsed."));
        }

        var userPublicId = principalResult.FindFirst(JwtClaimNames.UserId)?.Value;
        var jwtVersion = principalResult.FindFirst(JwtClaimNames.JwtVersion)?.Value;
        var tokenType = principalResult.FindFirst(JwtClaimNames.TokenType)?.Value;

        var tenantPublicId = principalResult.FindFirst(JwtClaimNames.Tenant)?.Value;
        var userRoleInTenant = principalResult.FindFirst(JwtClaimNames.Role)?.Value;
        
        var hasPlatformAccess = principalResult.FindFirst(JwtClaimNames.PlatformAccess)?.Value;
        var platformRole = principalResult.FindFirst(JwtClaimNames.PlatformRole)?.Value;
        
        if (string.IsNullOrWhiteSpace(userPublicId) ||
            string.IsNullOrWhiteSpace(jwtVersion) ||
            string.IsNullOrWhiteSpace(tokenType))
        {
            return Task.FromResult(ServiceResult<JwtParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Jwt claims are missing."));
        }

        if (!Enum.TryParse<JwtTokenType>(tokenType, out var parsedTokenType))
        {
            return Task.FromResult(ServiceResult<JwtParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                $"Unknown token type: {tokenType}."));
        }

        if (parsedTokenType == JwtTokenType.TenantAccessToken &&
            (string.IsNullOrWhiteSpace(tenantPublicId) ||
             string.IsNullOrWhiteSpace(userRoleInTenant)))
        {
            return Task.FromResult(ServiceResult<JwtParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Tenant access token claims are missing."));
        }
        
        if (parsedTokenType == JwtTokenType.PlatformAccessToken &&
            (string.IsNullOrWhiteSpace(hasPlatformAccess) ||
             hasPlatformAccess != "true" ||
             string.IsNullOrWhiteSpace(platformRole)))
        {
            return Task.FromResult(ServiceResult<JwtParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Platform access token claims are missing."));
        }

        var result = new JwtParseDto
        {
            UserPublicId = userPublicId,
            JwtVersion = jwtVersion,
            TenantPublicId = tenantPublicId ?? string.Empty,
            UserRoleInTenant = userRoleInTenant ?? string.Empty,
            TokenType = parsedTokenType,
            PlatformRole = platformRole ?? string.Empty,
            HasPlatformAccess = hasPlatformAccess == "true"
        };

        return Task.FromResult(ServiceResult<JwtParseDto>.Ok(
            result,
            ResultCodes.Token.JwtTokenParseSuccess,
            "Jwt claims parsed successfully."));
    }

    public Task<ServiceResult<InvitationParseDto>> ParseInvitationTokenAsync(string token)
    {
        var (principalResult, resultCode) = GetPrincipalFromTokenInner(token);

        if (principalResult == null)
        {
            return Task.FromResult(ServiceResult<InvitationParseDto>.Fail(
                resultCode,
                "Token could not be parsed."));
        }

        var invitationPublicId = principalResult.FindFirst(JwtClaimNames.InvitationPublicId)?.Value;
        var invitationVersion = principalResult.FindFirst(JwtClaimNames.InvitationVersion)?.Value;
        var tokenType = principalResult.FindFirst(JwtClaimNames.TokenType)?.Value;

        if (string.IsNullOrWhiteSpace(invitationPublicId) ||
            string.IsNullOrWhiteSpace(invitationVersion) ||
            string.IsNullOrWhiteSpace(tokenType))
        {
            return Task.FromResult(ServiceResult<InvitationParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Invitation token claims are missing."));
        }

        if (!Enum.TryParse<JwtTokenType>(tokenType, out var parsedTokenType) ||
            parsedTokenType != JwtTokenType.InvitationToken)
        {
            return Task.FromResult(ServiceResult<InvitationParseDto>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Invalid invitation token type."));
        }

        var result = new InvitationParseDto
        {
            InvitationPublicId = invitationPublicId,
            InvitationVersion = invitationVersion,
            TokenType = JwtTokenType.InvitationToken
        };

        return Task.FromResult(ServiceResult<InvitationParseDto>.Ok(
            result,
            ResultCodes.Token.InvitationTokenParseSuccess,
            "Invitation token parsed successfully."));
    }

    public PlatformTokenDto GeneratePlatformAccessToken(JwtClaimSource claimSource)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, claimSource.UserPublicId),
            new(JwtClaimNames.JwtVersion, claimSource.JwtVersion),
            new(JwtClaimNames.TokenType, JwtTokenType.PlatformAccessToken.ToString())
        };

        if (claimSource.HasPlatformAccess)
        {
            claims.Add(new Claim("platform_access", "true"));
        }

        if (!string.IsNullOrWhiteSpace(claimSource.PlatformRole))
        {
            claims.Add(new Claim("platform_role", claimSource.PlatformRole));
        }

        var token = GenerateToken(
            claims,
            JwtTokenType.PlatformAccessToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes));

        return new PlatformTokenDto
        {
            PlatformAccessToken = token,
            PlatformRole = claimSource.PlatformRole
        };
    }

    private string GenerateToken(
        IEnumerable<Claim> claims,
        JwtTokenType tokenType,
        DateTime expires)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var allClaims = claims.ToList();

        if (!allClaims.Any(x => x.Type == JwtClaimNames.TokenType))
        {
            allClaims.Add(new Claim(JwtClaimNames.TokenType, tokenType.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: allClaims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (ClaimsPrincipal?, string) GetPrincipalFromTokenInner(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret))
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is JwtSecurityToken jwtSecurityToken &&
                jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return (principal, ResultCodes.Token.JwtTokenParseSuccess);
            }

            return (null, ResultCodes.Token.JwtTokenParseFailed);
        }
        catch (SecurityTokenExpiredException)
        {
            return (null, ResultCodes.Token.JwtTokenExpired);
        }
        catch (SecurityTokenException)
        {
            return (null, ResultCodes.Token.JwtTokenInvalidForParse);
        }
        catch
        {
            return (null, ResultCodes.Token.JwtTokenInvalidForParse);
        }
    }
}