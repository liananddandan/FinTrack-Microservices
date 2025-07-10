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
                claims.Add(new Claim(JwtTokenCustomKeys.AccessTokenTypeKey, JwtTokenCustomKeys.AccessTokenTypeValue));
                break;
            case JwtTokenType.RefreshToken:
                claims.Add(new Claim(JwtTokenCustomKeys.RefreshTokenTypeKey, JwtTokenCustomKeys.RefreshTokenTypeValue));
                break;
            case JwtTokenType.FirstLoginToken:
                claims.Add(new Claim(JwtTokenCustomKeys.FirstLoginTokenKey, JwtTokenCustomKeys.FirstLoginTokenValue));
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
        var principalResult = GetPrincipalFromTokenInner(oldRefreshToken);
        if (principalResult is null)
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid,
                "Invalid refresh token.");
        }

        var tokenType = principalResult.FindFirst(JwtTokenCustomKeys.RefreshTokenTypeKey)?.Value;
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

        var user = await userService.GetUserByPublicIdInnerAsync(userPublicId);
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

        var role = await userService.GetRoleInnerAsync(user);
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
        var result = GetPrincipalFromTokenInner(token);
        var serviceResult = result == null 
            ? ServiceResult<ClaimsPrincipal?>.Fail(ResultCodes.Token.JwtTokenInvalidForParse, "Token Could not be parsed.")
            : ServiceResult<ClaimsPrincipal?>.Ok(result, ResultCodes.Token.JwtTokenParseSuccess, "Token parsed successfully.");
        return Task.FromResult(serviceResult);
    }

    private JwtTokenPair GenerateJwtTokenPairInner(JwtClaimSource jwtClaimSource)
    {
        var baseClaims = GetBaseClaims(jwtClaimSource);
        var accessTokenClaims = new List<Claim>(baseClaims)
        {
            new (JwtTokenCustomKeys.AccessTokenTypeKey, JwtTokenCustomKeys.AccessTokenTypeValue)
        };

        var refreshTokenClaims = new List<Claim>(baseClaims)
        {
            new (JwtTokenCustomKeys.RefreshTokenTypeKey, JwtTokenCustomKeys.RefreshTokenTypeValue)
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

    private ClaimsPrincipal? GetPrincipalFromTokenInner(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
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
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtTokenInner(List<Claim> claims, JwtTokenType type)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtOptions.Value.Issuer,
            jwtOptions.Value.Audience,
            claims,
            DateTime.UtcNow,
            (type == JwtTokenType.AccessToken)
                ? DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes) 
                : DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
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