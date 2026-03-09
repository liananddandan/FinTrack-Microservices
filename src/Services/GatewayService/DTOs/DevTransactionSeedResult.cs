namespace GatewayService.DTOs;

public record DevTransactionSeedResult(
    int DonationCount,
    int ProcurementCount,
    IReadOnlyList<string> CreatedTransactionPublicIds
);
