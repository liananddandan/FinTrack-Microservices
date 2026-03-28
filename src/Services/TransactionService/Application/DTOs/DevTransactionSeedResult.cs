namespace TransactionService.Application.DTOs;

public record DevTransactionSeedResult(
    int DonationCount,
    int ProcurementCount,
    IReadOnlyList<string> CreatedTransactionPublicIds
);
