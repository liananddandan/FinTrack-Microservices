namespace IdentityService.Application.Accounts.Dtos;

public sealed record CreateEmailVerificationTokenResult(
    string RawToken,
    DateTime ExpiresAtUtc);