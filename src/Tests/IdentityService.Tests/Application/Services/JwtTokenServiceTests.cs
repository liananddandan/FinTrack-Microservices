using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Options;
using SharedKernel.Common.Results;
using Xunit;

namespace IdentityService.Tests.Application.Services;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            Secret = "super-secret-key-for-unit-tests-1234567890",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        };
        
        Mock<ILogger<JwtTokenService>> logger = new Mock<ILogger<JwtTokenService>>();

        var options = Options.Create(_jwtOptions);
        _sut = new JwtTokenService(options, logger.Object);
    }

    [Fact]
    public void GenerateAccountAccessToken_Should_Return_Valid_Jwt()
    {
        var user = BuildUser();

        var token = _sut.GenerateAccountAccessToken(user);

        token.Should().NotBeNullOrWhiteSpace();

        var jwt = ReadJwt(token);

        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.UserId && x.Value == user.PublicId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.JwtVersion && x.Value == user.JwtVersion.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.TokenType && x.Value == JwtTokenType.AccountAccessToken.ToString());

        jwt.Claims.Should().NotContain(x => x.Type == JwtClaimNames.Tenant);
        jwt.Claims.Should().NotContain(x => x.Type == JwtClaimNames.Role);
    }

    [Fact]
    public void GenerateTenantAccessToken_Should_Return_Valid_Jwt_With_Tenant_Claims()
    {
        var user = BuildUser();
        var membership = BuildMembership();

        var token = _sut.GenerateTenantAccessToken(user, membership);

        token.Should().NotBeNullOrWhiteSpace();

        var jwt = ReadJwt(token);

        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.UserId && x.Value == user.PublicId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.JwtVersion && x.Value == user.JwtVersion.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.TokenType && x.Value == JwtTokenType.TenantAccessToken.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.Tenant && x.Value == membership.Tenant.PublicId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.Role && x.Value == membership.Role.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Valid_Jwt_Without_Tenant_Claims()
    {
        var user = BuildUser();

        var token = _sut.GenerateRefreshToken(user);

        token.Should().NotBeNullOrWhiteSpace();

        var jwt = ReadJwt(token);

        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.UserId && x.Value == user.PublicId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.JwtVersion && x.Value == user.JwtVersion.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.TokenType && x.Value == JwtTokenType.RefreshToken.ToString());

        jwt.Claims.Should().NotContain(x => x.Type == JwtClaimNames.Tenant);
        jwt.Claims.Should().NotContain(x => x.Type == JwtClaimNames.Role);
    }

    [Fact]
    public void GenerateInvitationToken_Should_Return_Valid_Jwt_With_Invitation_Claims()
    {
        var invitation = BuildInvitation();

        var token = _sut.GenerateInvitationToken(invitation);

        token.Should().NotBeNullOrWhiteSpace();

        var jwt = ReadJwt(token);

        jwt.Claims.Should().Contain(x =>
            x.Type == JwtClaimNames.InvitationPublicId &&
            x.Value == invitation.PublicId.ToString());

        jwt.Claims.Should().Contain(x =>
            x.Type == JwtClaimNames.InvitationVersion &&
            x.Value == invitation.Version.ToString());

        jwt.Claims.Should().Contain(x =>
            x.Type == JwtClaimNames.TokenType &&
            x.Value == JwtTokenType.InvitationToken.ToString());
    }

    [Fact]
    public async Task GetPrincipalFromTokenAsync_Should_Return_Success_When_Token_Is_Valid()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccountAccessToken(user);

        var result = await _sut.GetPrincipalFromTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FindFirst(JwtClaimNames.UserId)!.Value.Should().Be(user.PublicId.ToString());
    }

    [Fact]
    public async Task GetPrincipalFromTokenAsync_Should_Return_Fail_When_Token_Is_Invalid()
    {
        var result = await _sut.GetPrincipalFromTokenAsync("invalid-token");

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenInvalidForParse);
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Parse_AccountAccessToken_Successfully()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccountAccessToken(user);

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.UserPublicId.Should().Be(user.PublicId.ToString());
        result.Data.JwtVersion.Should().Be(user.JwtVersion.ToString());
        result.Data.TokenType.Should().Be(JwtTokenType.AccountAccessToken);
        result.Data.TenantPublicId.Should().BeEmpty();
        result.Data.UserRoleInTenant.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Parse_TenantAccessToken_Successfully()
    {
        var user = BuildUser();
        var membership = BuildMembership();
        var token = _sut.GenerateTenantAccessToken(user, membership);

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.UserPublicId.Should().Be(user.PublicId.ToString());
        result.Data.JwtVersion.Should().Be(user.JwtVersion.ToString());
        result.Data.TokenType.Should().Be(JwtTokenType.TenantAccessToken);
        result.Data.TenantPublicId.Should().Be(membership.Tenant.PublicId.ToString());
        result.Data.UserRoleInTenant.Should().Be(membership.Role.ToString());
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Parse_RefreshToken_Successfully()
    {
        var user = BuildUser();
        var token = _sut.GenerateRefreshToken(user);

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.UserPublicId.Should().Be(user.PublicId.ToString());
        result.Data.JwtVersion.Should().Be(user.JwtVersion.ToString());
        result.Data.TokenType.Should().Be(JwtTokenType.RefreshToken);
        result.Data.TenantPublicId.Should().BeEmpty();
        result.Data.UserRoleInTenant.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Return_Fail_When_TenantToken_Missing_Tenant_Claim()
    {
        var token = BuildTokenWithoutTenantClaim();

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenClaimMissing);
        result.Message.Should().Be("Tenant access token claims are missing.");
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Return_Fail_When_TokenType_Is_Unknown()
    {
        var token = BuildTokenWithUnknownType();

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenClaimMissing);
        result.Message.Should().Contain("Unknown token type");
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Return_Fail_When_Required_Claims_Are_Missing()
    {
        var token = BuildTokenWithoutUserId();

        var result = await _sut.ParseJwtTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenClaimMissing);
        result.Message.Should().Be("Jwt claims are missing.");
    }

    [Fact]
    public async Task ParseJwtTokenAsync_Should_Return_Fail_When_Token_Is_Invalid()
    {
        var result = await _sut.ParseJwtTokenAsync("invalid-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenInvalidForParse);
    }

    [Fact]
    public async Task ParseInvitationTokenAsync_Should_Parse_InvitationToken_Successfully()
    {
        var invitation = BuildInvitation();
        var token = _sut.GenerateInvitationToken(invitation);

        var result = await _sut.ParseInvitationTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.InvitationPublicId.Should().Be(invitation.PublicId.ToString());
        result.Data.InvitationVersion.Should().Be(invitation.Version.ToString());
        result.Data.TokenType.Should().Be(JwtTokenType.InvitationToken);
    }

    [Fact]
    public async Task ParseInvitationTokenAsync_Should_Return_Fail_When_Claims_Are_Missing()
    {
        var token = BuildTokenWithoutInvitationClaims();

        var result = await _sut.ParseInvitationTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenClaimMissing);
        result.Message.Should().Be("Invitation token claims are missing.");
    }

    [Fact]
    public async Task ParseInvitationTokenAsync_Should_Return_Fail_When_TokenType_Is_Not_InvitationToken()
    {
        var user = BuildUser();
        var token = _sut.GenerateAccountAccessToken(user);

        var result = await _sut.ParseInvitationTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenClaimMissing);
        result.Message.Should().Be("Invitation token claims are missing.");
    }

    [Fact]
    public async Task ParseInvitationTokenAsync_Should_Return_Fail_When_Token_Is_Invalid()
    {
        var result = await _sut.ParseInvitationTokenAsync("invalid-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Token.JwtTokenInvalidForParse);
    }

    private ApplicationUser BuildUser()
    {
        return new ApplicationUser
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user@test.com",
            JwtVersion = 1
        };
    }

    private TenantMembership BuildMembership()
    {
        return new TenantMembership
        {
            Id = 1,
            TenantId = 100,
            Tenant = new Tenant
            {
                Id = 100,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            UserId = 1,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
    }

    private TenantInvitation BuildInvitation()
    {
        return new TenantInvitation
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Email = "invitee@test.com",
            TenantId = 100,
            Tenant = new Tenant
            {
                Id = 100,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            Version = 1,
            CreatedByUserId = 999
        };
    }

    private JwtSecurityToken ReadJwt(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }

    private string BuildTokenWithoutTenantClaim()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, Guid.NewGuid().ToString()),
            new(JwtClaimNames.JwtVersion, "1"),
            new(JwtClaimNames.TokenType, JwtTokenType.TenantAccessToken.ToString()),
            new(JwtClaimNames.Role, TenantRole.Admin.ToString())
        };

        return BuildRawToken(claims, DateTime.UtcNow.AddMinutes(10));
    }

    private string BuildTokenWithUnknownType()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, Guid.NewGuid().ToString()),
            new(JwtClaimNames.JwtVersion, "1"),
            new(JwtClaimNames.TokenType, "UnknownTokenType")
        };

        return BuildRawToken(claims, DateTime.UtcNow.AddMinutes(10));
    }

    private string BuildTokenWithoutUserId()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.JwtVersion, "1"),
            new(JwtClaimNames.TokenType, JwtTokenType.AccountAccessToken.ToString())
        };

        return BuildRawToken(claims, DateTime.UtcNow.AddMinutes(10));
    }

    private string BuildTokenWithoutInvitationClaims()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.TokenType, JwtTokenType.InvitationToken.ToString())
        };

        return BuildRawToken(claims, DateTime.UtcNow.AddMinutes(10));
    }

    private string BuildRawToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}