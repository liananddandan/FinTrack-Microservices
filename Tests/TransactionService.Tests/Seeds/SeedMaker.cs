using TransactionService.Common.Status;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure;

namespace TransactionService.Tests.Seeds;

public class SeedMaker
{
    public static async Task InitializeAsync(TransactionDbContext dbContext)
    {
        await AddTransactionForQueryAsync(dbContext);
        await AddTransactionForQueryByPage(dbContext);
    }

    private static async Task AddTransactionForQueryByPage(TransactionDbContext dbContext)
    {
        var transactions = new List<Transaction>();
        for (var i = 2; i < 22; i++)
        {
            var transaction = new Transaction()
            {
                TenantPublicId = "11111111-1111-1111-1111-111111111112",
                UserPublicId = "11111111-1111-1111-1111-111111111112",
                Amount = new decimal(i * 12.34),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = "Page_test"
            };
            transactions.Add(transaction);
        }

        await dbContext.Transactions.AddRangeAsync(transactions);
        await dbContext.SaveChangesAsync();
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