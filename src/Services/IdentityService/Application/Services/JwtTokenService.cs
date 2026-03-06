using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Options;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services;

public class JwtTokenService(
    IUserDomainService userService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<JwtTokenService> logger) : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    private JwtTokenPair GenerateJwtTokenPairInner(JwtClaimSource jwtClaimSource)
    {
        var baseClaims = GetBaseClaims(jwtClaimSource);
        var accessTokenClaims = new List<Claim>(baseClaims)
        {
            new(JwtTokenCustomKeys.TokenType, JwtTokenCustomKeys.AccessTokenTypeValue)
        };

        var refreshTokenClaims = new List<Claim>(baseClaims)
        {
            new(JwtTokenCustomKeys.TokenType, JwtTokenCustomKeys.RefreshTokenTypeValue)
        };
        var accessToken = GenerateJwtTokenInner(accessTokenClaims, JwtTokenType.AccessToken);
        var refreshToken = GenerateJwtTokenInner(refreshTokenClaims, JwtTokenType.RefreshToken);
        var jwtTokenPair = new JwtTokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
        return jwtTokenPair;
    }

    private string GenerateJwtTokenInner(List<Claim> claims, JwtTokenType type)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expire = type switch
        {
            JwtTokenType.AccessToken => DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes),
            JwtTokenType.RefreshToken => DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
            JwtTokenType.InvitationToken => DateTime.UtcNow.AddDays(jwtOptions.Value.InvitationTokenExpirationDays),
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported token type")
        };

        var token = new JwtSecurityToken(
            jwtOptions.Value.Issuer,
            jwtOptions.Value.Audience,
            claims,
            DateTime.UtcNow,
            expire,
            credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private List<Claim> GetBaseClaims(JwtClaimSource jwtClaimSource)
    {
        var claims = new List<Claim>()
        {
            new(JwtTokenCustomKeys.UserPublicIdKey, jwtClaimSource.UserPublicId),
            new(JwtTokenCustomKeys.JwtVersionKey, jwtClaimSource.JwtVersion),
            new(JwtTokenCustomKeys.TenantPublicIdKey, jwtClaimSource.TenantPublicId),
            new(JwtTokenCustomKeys.UserRoleInTenant, jwtClaimSource.UserRoleInTenant)
        };
        return claims;
    }

    public Task<ServiceResult<string>> GenerateJwtTokenAsync(JwtClaimSource jwtClaimSource, JwtTokenType type)
    {
        var claims = GetBaseClaims(jwtClaimSource);
        switch (type)
        {
            case JwtTokenType.AccessToken:
                claims.Add(new Claim(JwtTokenCustomKeys.TokenType, JwtTokenCustomKeys.AccessTokenTypeValue));
                break;
            case JwtTokenType.RefreshToken:
                claims.Add(new Claim(JwtTokenCustomKeys.TokenType, JwtTokenCustomKeys.RefreshTokenTypeValue));
                break;
        }

        return Task.FromResult(ServiceResult<string>.Ok(GenerateJwtTokenInner(claims, type),
            ResultCodes.Token.GenerateJwtTokenSuccess, $"Generate Jwt {type} Token Success."));
    }

    public Task<ServiceResult<JwtTokenPair>> GenerateJwtTokenPairAsync(JwtClaimSource jwtClaimSource)
    {
        var jwtTokenPair = GenerateJwtTokenPairInner(jwtClaimSource);
        return Task.FromResult(ServiceResult<JwtTokenPair>.Ok(jwtTokenPair,
            ResultCodes.Token.GenerateJwtTokenPairSuccess,
            "Generate Jwt Token Pair Success"));
    }

    public async Task<ServiceResult<JwtTokenPair>> RefreshJwtTokenPairAsync(string oldRefreshToken)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token)
    {
        var (result, resultCode) = GetPrincipalFromTokenInner(token);
        var serviceResult = result == null
            ? ServiceResult<ClaimsPrincipal?>.Fail(resultCode,
                "Token Could not be parsed.")
            : ServiceResult<ClaimsPrincipal?>.Ok(result, resultCode,
                "Token parsed successfully.");
        return Task.FromResult(serviceResult);
    }

    public Task<ServiceResult<string>> GenerateInvitationTokenAsync(InvitationClaimSource invitationClaimSource)
    {
        var claims = new List<Claim>()
        {
            new(JwtTokenCustomKeys.InvitationPublicIdkey, invitationClaimSource.InvitationPublicId),
            new(JwtTokenCustomKeys.InvitationVersionKey, invitationClaimSource.InvitationVersion)
        };
        return Task.FromResult(ServiceResult<string>
            .Ok(GenerateJwtTokenInner(claims, JwtTokenType.InvitationToken),
                ResultCodes.Token.GenerateInvitationTokenSuccess,
                "Invitation token generated successfully."));
    }

    public Task<ServiceResult<InvitationParseResult>> ParseInvitationTokenAsync(string token)
    {
        var (principalResult, resultCode) = GetPrincipalFromTokenInner(token);
        if (principalResult is null)
        {
            return Task.FromResult(ServiceResult<InvitationParseResult>.Fail(resultCode,
                "Token could not be parsed."));
        }

        var invitationPublicId = principalResult.FindFirst(JwtTokenCustomKeys.InvitationPublicIdkey)?.Value;
        var invitationVersion = principalResult.FindFirst(JwtTokenCustomKeys.InvitationVersionKey)?.Value;
        if (string.IsNullOrEmpty(invitationPublicId)
            || string.IsNullOrEmpty(invitationVersion))
        {
            return Task.FromResult(ServiceResult<InvitationParseResult>
                .Fail(ResultCodes.Token.JwtTokenClaimMissing, "Jwt claims are missing."));
        }

        var invitationParseResult = new InvitationParseResult()
        {
            InvitationPublicId = invitationPublicId,
            InvitationVersion = invitationVersion,
            TokenType = JwtTokenType.InvitationToken
        };
        return Task.FromResult(ServiceResult<InvitationParseResult>.Ok(invitationParseResult,
            ResultCodes.Token.InvitationTokenParseSuccess,
            "Invitation Claim parsed successfully."));
    }

    // updated interface!!!
    public string GenerateAccessToken(ApplicationUser user, TenantMembership membership)
    {
        if (membership is null)
        {
            throw new InvalidOperationException("User has no active tenant membership.");
        }

        var claims = new List<Claim>
        {
            new Claim(JwtClaimNames.UserId, user.PublicId.ToString()),
            new Claim(JwtClaimNames.JwtVersion, user.JwtVersion.ToString()),
            new Claim(JwtClaimNames.Tenant, membership.Tenant.PublicId.ToString()),
            new Claim(JwtClaimNames.Role, membership.Role.ToString()),
        };

        return GenerateToken(
            claims,
            JwtTokenType.AccessToken,
            DateTime.UtcNow.AddDays(_jwtOptions.AccessTokenExpirationMinutes));
    }
    
    public string GenerateRefreshToken(ApplicationUser user, TenantMembership membership)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtClaimNames.UserId, user.PublicId.ToString()),
            new Claim(JwtClaimNames.JwtVersion, user.JwtVersion.ToString()),
            new Claim(JwtClaimNames.Tenant, membership.Tenant.PublicId.ToString()),
            new Claim(JwtClaimNames.Role, membership.Role.ToString()),
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
            new Claim(JwtClaimNames.InvitationPublicId, invitation.PublicId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, invitation.Email),
            new Claim(JwtClaimNames.Tenant, invitation.Tenant.PublicId.ToString()),
            new Claim(JwtClaimNames.Role, invitation.Role.ToString())
        };

        return GenerateToken(
            claims,
            JwtTokenType.InvitationToken,
            DateTime.UtcNow.AddDays(_jwtOptions.InvitationTokenExpirationDays));
    }

    public Task<ServiceResult<JwtParseResult>> ParseJwtTokenAsync(string token)
    {
        var (principalResult, resultCode) = GetPrincipalFromTokenInner(token);
        if (principalResult is null)
        {
            return Task.FromResult(ServiceResult<JwtParseResult>.Fail(
                resultCode,
                "Token could not be parsed."));
        }

        var userPublicId = principalResult.FindFirst(JwtClaimNames.UserId)?.Value;
        var tenantPublicId = principalResult.FindFirst(JwtClaimNames.Tenant)?.Value;
        var jwtVersion = principalResult.FindFirst(JwtClaimNames.JwtVersion)?.Value;
        var userRoleInTenant = principalResult.FindFirst(JwtClaimNames.Role)?.Value;
        var tokenType = principalResult.FindFirst(JwtClaimNames.TokenType)?.Value;
        logger.LogInformation(
            "ParseJwtTokenAsync claims. UserPublicId={UserPublicId}, TenantPublicId={TenantPublicId}, JwtVersion={JwtVersion}, Role={Role}, TokenType={TokenType}",
            userPublicId, tenantPublicId, jwtVersion, userRoleInTenant, tokenType);
        if (string.IsNullOrEmpty(userPublicId)
            || string.IsNullOrEmpty(tenantPublicId)
            || string.IsNullOrEmpty(jwtVersion)
            || string.IsNullOrEmpty(userRoleInTenant)
            || string.IsNullOrEmpty(tokenType))
        {
            return Task.FromResult(ServiceResult<JwtParseResult>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                "Jwt claims are missing."));
        }

        if (!Enum.TryParse<JwtTokenType>(tokenType, out var type))
        {
            return Task.FromResult(ServiceResult<JwtParseResult>.Fail(
                ResultCodes.Token.JwtTokenClaimMissing,
                $"Unknown token type: {tokenType}."));
        }

        var jwtParseResult = new JwtParseResult
        {
            JwtVersion = jwtVersion,
            UserPublicId = userPublicId,
            TenantPublicId = tenantPublicId,
            UserRoleInTenant = userRoleInTenant,
            TokenType = type
        };

        return Task.FromResult(ServiceResult<JwtParseResult>.Ok(
            jwtParseResult,
            ResultCodes.Token.JwtTokenParseSuccess,
            "Jwt Claim parsed successfully."));
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

        allClaims.Add(new Claim("type", tokenType.ToString()));

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
        var tokenHandler = new JwtSecurityTokenHandler()
        {
            MapInboundClaims = false
        };
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret))
        };
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken
                && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
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