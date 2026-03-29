namespace SharedKernel.Contracts.Dev;


public record DevTransactionSeedResult(
    int CategoryCount,
    int ProductCount,
    int OrderCount,
    IReadOnlyList<string> CreatedOrderPublicIds);