namespace TransactionService.Api.Dev.Contracts;

public record DevTransactionSeedResult(
    int DonationCount,
    int ProcurementCount,
    IReadOnlyList<string> CreatedTransactionPublicIds
);
