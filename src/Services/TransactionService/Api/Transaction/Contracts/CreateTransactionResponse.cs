using TransactionService.Common.Status;

namespace TransactionService.Api.Transaction.Contracts;

public record CreateTransactionResponse(
    string TransactionPublicId,
    decimal Amount,
    string Currency,
    TransStatus TranStatus,
    RiskStatus RiskStatus,
    DateTime CreatedAt);