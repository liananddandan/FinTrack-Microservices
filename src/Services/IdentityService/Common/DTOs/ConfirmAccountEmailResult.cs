namespace IdentityService.Common.DTOs;

public record ConfirmAccountEmailResult(Guid UserPublicId, bool IsConfirmed);