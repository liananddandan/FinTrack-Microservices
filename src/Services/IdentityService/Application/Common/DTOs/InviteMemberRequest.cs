namespace IdentityService.Application.Common.DTOs;

public class InviteMemberRequest
{
    public required string Email { get; set; }

    public required string Role { get; set; }
}