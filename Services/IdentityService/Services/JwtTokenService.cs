using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Common.DTOs;
using IdentityService.Common.Options;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class JwtTokenService(IUserDomainService userService, IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
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
            case JwtTokenType.FirstLoginToken:
                claims.Add(new Claim(JwtTokenCustomKeys.TokenType, JwtTokenCustomKeys.FirstLoginTokenValue));
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
        var (principalResult, _) = GetPrincipalFromTokenInner(oldRefreshToken);
        if (principalResult is null)
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid,
                "Invalid refresh token.");
        }

        var tokenType = principalResult.FindFirst(JwtTokenCustomKeys.TokenType)?.Value;
        if (!JwtTokenCustomKeys.RefreshTokenTypeValue.Equals(tokenType))
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedTokenTypeInvalid,
                "Invalid refresh token type.");
        }

        var userPublicId = principalResult.FindFirst(JwtTokenCustomKeys.UserPublicIdKey)?.Value;
        var tenantPublicId = principalResult.FindFirst(JwtTokenCustomKeys.TenantPublicIdKey)?.Value;
        var jwtVersion = principalResult.FindFirst(JwtTokenCustomKeys.JwtVersionKey)?.Value;
        if (string.IsNullOrEmpty(userPublicId) || string.IsNullOrEmpty(tenantPublicId) ||
            string.IsNullOrEmpty(jwtVersion))
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedClaimMissing,
                "Invalid refresh token without valid claims.");
        }

        var user = await userService.GetUserByPublicIdIncludeTenantAsync(userPublicId);
        if (user is null)
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedClaimUserNotFound,
                "Invalid refresh token with invalid user.");
        }

        if (!long.TryParse(jwtVersion, out var jwtVersionL) || user.JwtVersion > jwtVersionL)
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedVersionInvalid,
                "Invalid refresh token version.");
        }

        if (!Guid.TryParse(tenantPublicId, out var tPublicId)
            || user.Tenant == null
            || !user.Tenant.PublicId.Equals(tPublicId))
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedClaimTenantIdInvalid,
                "Invalid tenant ID.");
        }

        var role = await userService.GetUserRoleInnerAsync(user);
        if (role is null)
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedRoleInvalid,
                "Could not find role for user.");
        }

        var jwtClaimSource = new JwtClaimSource
        {
            JwtVersion = user.JwtVersion.ToString(),
            UserPublicId = user.PublicId.ToString(),
            TenantPublicId = user.Tenant.PublicId.ToString(),
            UserRoleInTenant = role
        };

        var tokenPair = GenerateJwtTokenPairInner(jwtClaimSource);
        return ServiceResult<JwtTokenPair>.Ok(tokenPair, ResultCodes.Token.RefreshJwtTokenSuccess,
            "Refresh token success");
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

    public Task<ServiceResult<JwtParseResult>> ParseJwtTokenAsync(string token)
    {
        var (principalResult, resultCode) = GetPrincipalFromTokenInner(token);
        if (principalResult is null)
        {
            return Task.FromResult(ServiceResult<JwtParseResult>.Fail(resultCode,
                "Token could not be parsed."));
        }

        var userPublicId = principalResult.FindFirst(JwtTokenCustomKeys.UserPublicIdKey)?.Value;
        var tenantPublicId = principalResult.FindFirst(JwtTokenCustomKeys.TenantPublicIdKey)?.Value;
        var jwtVersion = principalResult.FindFirst(JwtTokenCustomKeys.JwtVersionKey)?.Value;
        var userRoleInTenant = principalResult.FindFirst(JwtTokenCustomKeys.UserRoleInTenant)?.Value;
        var tokenType = principalResult.FindFirst(JwtTokenCustomKeys.TokenType)?.Value;
        
        if (string.IsNullOrEmpty(userPublicId) 
            || string.IsNullOrEmpty(tenantPublicId) 
            || string.IsNullOrEmpty(jwtVersion) 
            || string.IsNullOrEmpty(userRoleInTenant)
            || string.IsNullOrEmpty(tokenType))
        {
            return Task.FromResult(ServiceResult<JwtParseResult>
                .Fail(ResultCodes.Token.JwtTokenClaimMissing, "Jwt claims are missing."));
        }

        var type = tokenType switch
        {
            JwtTokenCustomKeys.AccessTokenTypeValue => JwtTokenType.AccessToken,
            JwtTokenCustomKeys.RefreshTokenTypeValue => JwtTokenType.RefreshToken,
            JwtTokenCustomKeys.FirstLoginTokenValue => JwtTokenType.FirstLoginToken,
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), $"Unknow token type{tokenType}.")
        };
        var jwtParseResult = new JwtParseResult()
            {
                JwtVersion = jwtVersion,
                UserPublicId = userPublicId,
                TenantPublicId = tenantPublicId,
                UserRoleInTenant = userRoleInTenant,
                TokenType = type
            };
        return Task.FromResult(ServiceResult<JwtParseResult>.Ok(jwtParseResult, 
            ResultCodes.Token.JwtTokenParseSuccess, "Jwt Claim parsed successfully."));
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

    private (ClaimsPrincipal?, string) GetPrincipalFromTokenInner(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
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

    private string GenerateJwtTokenInner(List<Claim> claims, JwtTokenType type)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expire = type switch
        {
            JwtTokenType.AccessToken => DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes),
            JwtTokenType.RefreshToken => DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
            JwtTokenType.FirstLoginToken => DateTime.UtcNow.AddMinutes(jwtOptions.Value
                .FirstLoginChangePasswordExpirationMinutes),
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
}