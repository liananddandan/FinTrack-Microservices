namespace TransactionService.Common.Requests;

public record CreateTransactionRequest(decimal Amount, string Currency, string? Description);