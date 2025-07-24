using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure;
using TransactionService.Repositories;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Repositories;

public class TransactionRepoTests
{
    private class TestDbContext(DbContextOptions<TransactionDbContext> options) : TransactionDbContext(options)
    {

    }
    
    [Theory, AutoMoqData]
    public async Task AddTransactionAsync_ShouldAddItemToDB(
        Transaction transaction)
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        
        // act
        await repo.AddTransactionAsync(transaction);
        await context.SaveChangesAsync();
        
        // assert
        var queryT = await context.Transactions
            .Where(t => t.TransactionPublicId == transaction.TransactionPublicId)
            .FirstOrDefaultAsync();
        queryT.Should().NotBeNull();
        queryT.Should().BeEquivalentTo(transaction);
    }
}