namespace IdentityService.Api.Tenants.Contracts;

public class InviteMemberRequest
{
    public required string Email { get; set; }

    public required string Role { get; set; }
}