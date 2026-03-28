using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Common.DTOs.Auth;

namespace TransactionService.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public const string UserIdHeader = "X-Test-UserId";
    public const string TenantIdHeader = "X-Test-TenantId";
    public const string RoleHeader = "X-Test-Role";

    public static readonly Guid DefaultUserPublicId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid DefaultTenantPublicId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public const string DefaultRole = "Admin";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasRole = Request.Headers.TryGetValue(RoleHeader, out var roleValues);
        var hasUser = Request.Headers.TryGetValue(UserIdHeader, out var userValues);
        var hasTenant = Request.Headers.TryGetValue(TenantIdHeader, out var tenantValues);

        if (!hasRole && !hasUser && !hasTenant)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = hasRole ? roleValues.FirstOrDefault() ?? "Member" : "Member";
        var userId = hasUser && Guid.TryParse(userValues.FirstOrDefault(), out var parsedUser)
            ? parsedUser
            : Guid.NewGuid();
        var tenantId = hasTenant && Guid.TryParse(tenantValues.FirstOrDefault(), out var parsedTenant)
            ? parsedTenant
            : Guid.Empty;

        var claims = new[]
        {
            new Claim(JwtClaimNames.UserId, userId.ToString()),
            new Claim(JwtClaimNames.Tenant, tenantId.ToString()),
            new Claim(JwtClaimNames.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}