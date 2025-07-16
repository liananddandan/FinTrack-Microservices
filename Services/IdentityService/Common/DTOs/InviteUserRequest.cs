namespace IdentityService.Common.DTOs;

public class InviteUserRequest
{
    public List<string> Emails { get; set; } = new();
}