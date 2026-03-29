namespace IdentityService.Application.Common.DTOs;

public record ConfirmAccountEmailDto(Guid UserPublicId, bool IsConfirmed);