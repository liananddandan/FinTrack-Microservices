using TransactionService.Common.Status;

namespace TransactionService.Common.Responses;

public record CreateTransactionResponse(
    string TransactionPublicId,
    decimal Amount,
    string Currency,
    TransactionStatus TranStatus,
    RiskStatus RiskStatus,
    DateTime CreatedAt);