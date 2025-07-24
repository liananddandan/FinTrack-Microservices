using TransactionService.Common.Status;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure;

namespace TransactionService.Tests.Seeds;

public class SeedMaker
{
    public static async Task InitializeAsync(TransactionDbContext dbContext)
    {
        await AddTransactionForQueryAsync(dbContext);
    }

    private static async Task AddTransactionForQueryAsync(TransactionDbContext dbContext)
    {
        var transaction = new Transaction()
        {
            TransactionPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TenantPublicId = "11111111-1111-1111-1111-111111111111",
            UserPublicId = "11111111-1111-1111-1111-111111111111",
            Amount = decimal.Parse("1.23"),
            Currency = "USD",
            TransStatus = TransStatus.Success,
            RiskStatus = RiskStatus.Pass,
            Description = "test_Query"
        };
        await dbContext.Transactions.AddAsync(transaction);
        await dbContext.SaveChangesAsync();
    }
}