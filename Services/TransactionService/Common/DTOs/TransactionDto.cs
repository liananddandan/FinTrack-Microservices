using TransactionService.Common.Status;

namespace TransactionService.Common.DTOs;

public record TransactionDto(string TransactionPublicId, 
    string TenantPublicId, 
    string UserPublicId, 
    decimal Amount, 
    string Currency,
    TransStatus Status,
    RiskStatus RiskStatus,
    string? Description,
    DateTime CreatedAt
    );