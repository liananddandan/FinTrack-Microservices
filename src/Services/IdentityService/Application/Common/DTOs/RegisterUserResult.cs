namespace IdentityService.Application.Common.DTOs;

public record RegisterUserResult(
    string UserPublicId,
    string Email,
    string UserName
);