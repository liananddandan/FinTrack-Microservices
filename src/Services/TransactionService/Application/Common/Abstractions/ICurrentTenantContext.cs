namespace TransactionService.Application.Common.Abstractions;

public interface ICurrentTenantContext
{
    Guid TenantPublicId { get; }
    string? TenantName { get; }
    
    Guid UserPublicId { get; }
    string? UserName { get; }
    string? UserEmail { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}