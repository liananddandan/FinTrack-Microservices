namespace TransactionService.Application.Common.Abstractions;

public interface ICurrentTenantContext
{
    Guid TenantPublicId { get; }
    Guid UserPublicId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}