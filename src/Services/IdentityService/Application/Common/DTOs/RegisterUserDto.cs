namespace IdentityService.Application.Common.DTOs;

public record RegisterUserDto(
    string UserPublicId,
    string Email,
    string UserName
);