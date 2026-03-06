namespace IdentityService.Application.Common.DTOs;

public class InviteUserRequest
{
    public List<string> Emails { get; set; } = new();
}