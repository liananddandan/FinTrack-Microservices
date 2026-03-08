namespace IdentityService.Application.Common.DTOs;

public record ConfirmAccountEmailResult(Guid UserPublicId, bool IsConfirmed);