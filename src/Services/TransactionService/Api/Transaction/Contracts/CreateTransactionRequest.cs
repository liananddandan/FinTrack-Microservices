namespace TransactionService.Api.Transaction.Contracts;

public record CreateTransactionRequest(decimal Amount, string Currency, string? Description);