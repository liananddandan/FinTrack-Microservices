using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TransactionService.Common.Status;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure;
using TransactionService.Repositories;
using TransactionService.Tests.Attributes;
using Xunit.Abstractions;

namespace TransactionService.Tests.Repositories;

public class TransactionRepoTests(ITestOutputHelper testOutputHelper)
{
    private class TestDbContext(DbContextOptions<TransactionDbContext> options)
        : TransactionDbContext(options)
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

    [Theory, AutoMoqData]
    public async Task GetTransactionByPublicIdAsync_ShouldReturnNull_WhenTransactionNotExist(
        string transactionPublicId,
        Transaction transaction)
    {
        // Arrange
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        await repo.AddTransactionAsync(transaction);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repo.GetTransactionByPublicIdAsync(transactionPublicId);
        
        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetTransactionByPublicIdAsync_ShouldReturnTransaction_WhenTransactionExist(
        Transaction transaction)
    {
        // Arrange
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        await repo.AddTransactionAsync(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetTransactionByPublicIdAsync(transaction.TransactionPublicId.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(transaction);
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnNull_IfNoExistInPublicIdOrTenantId()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 1; i < 10; i++)
        {
            var t = new Transaction(){
                TenantPublicId = "11111111-1111-1111-1111-111111111111",
                UserPublicId = "11111111-1111-1111-1111-111111111111",
                Amount = new decimal(15.234 * i),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = $"{i}"
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-11111111112",
            "11111111-1111-1111-1111-111111111112",
            null,
            null,
            1,
            10,
            "desc");
        
        // assert
        totalCount.Should().Be(0);
        data.Should().NotBeNull();
        data.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnNull_IfNoTransactionsExist()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-11111111111",
            "11111111-1111-1111-1111-111111111111",
            null,
            null,
            1,
            10,
            "desc");
        
        // assert
        totalCount.Should().Be(0);
        data.Should().NotBeNull();
        data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnAll_IfCountLessThanPageSize()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 1; i < 10; i++)
        {
            var t = new Transaction(){
               TenantPublicId = "11111111-1111-1111-1111-111111111111",
               UserPublicId = "11111111-1111-1111-1111-111111111111",
               Amount = new decimal(15.234 * i),
               Currency = "USD",
               TransStatus = TransStatus.Success,
               RiskStatus = RiskStatus.Pass,
               Description = $"{i}"
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            null,
            null,
            1,
            10,
            "desc");
        
        // assert
        totalCount.Should().Be(9);
        data.Should().NotBeNull();
        data.Should().BeEquivalentTo(transactions);
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnPageSizeItems_IfCountMoreThanPageSize()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var countSize = 17;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 0; i < countSize; i++)
        {
            var t = new Transaction(){
                TenantPublicId = "11111111-1111-1111-1111-111111111111",
                UserPublicId = "11111111-1111-1111-1111-111111111111",
                Amount = new decimal(15.234 * i),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = $"{i}"
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            null,
            null,
            1,
            10,
            "desc");
        
        // assert
        totalCount.Should().Be(countSize);
        data.Should().NotBeNull();
        data.Count.Should().Be(10);
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnSecondPageSizeItems_IfCountMoreThanPageSize()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var countSize = 17;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 0; i < countSize; i++)
        {
            var t = new Transaction(){
                TenantPublicId = "11111111-1111-1111-1111-111111111111",
                UserPublicId = "11111111-1111-1111-1111-111111111111",
                Amount = new decimal(15.234 * i),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = $"{i}"
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            null,
            null,
            2,
            10,
            "desc");
        
        // assert
        totalCount.Should().Be(countSize);
        data.Should().NotBeNull();
        data.Count.Should().Be(7);
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturnAsc_IfCountMoreThanPageSize()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var countSize = 17;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 0; i < countSize; i++)
        {
            var t = new Transaction(){
                TenantPublicId = "11111111-1111-1111-1111-111111111111",
                UserPublicId = "11111111-1111-1111-1111-111111111111",
                Amount = new decimal(15.234 * i),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = $"{i}"
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            null,
            null,
            2,
            10,
            "asc");
        
        // assert
        totalCount.Should().Be(countSize);
        data.Should().NotBeNull();
        data.Count.Should().Be(7);
        data.ForEach(t =>
        {
            testOutputHelper.WriteLine($"{t.CreatedAt}, {t.Id}, {t.TenantPublicId}, {t.Amount}, {t.Currency}");
        });
    }
    
    [Fact]
    public async Task GetTransactionsByPageAsync_ShouldReturn_IfCreateAtMoreThan()
    {
        var option = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var countSize = 17;
        await using var context = new TestDbContext(option);
        var repo = new TransactionRepo(context);
        var transactions = new List<Transaction>();
        for (int i = 0; i < countSize; i++)
        {
            var t = new Transaction(){
                TenantPublicId = "11111111-1111-1111-1111-111111111111",
                UserPublicId = "11111111-1111-1111-1111-111111111111",
                Amount = new decimal(15.234 * i),
                Currency = "USD",
                TransStatus = TransStatus.Success,
                RiskStatus = RiskStatus.Pass,
                Description = $"{i}",
                CreatedAt = DateTime.UtcNow.AddDays(i)
            };
            transactions.Add(t);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
        
        // act
        var (data, totalCount) = await repo.GetTransactionsByPageAsync(
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            DateTime.UtcNow.AddDays(11),
            null,
            1,
            10,
            "asc");
        
        // assert
        totalCount.Should().Be(5);
        data.Should().NotBeNull();
        data.Count.Should().Be(5);
        data.ForEach(t =>
        {
            testOutputHelper.WriteLine($"{t.CreatedAt}, {t.Id}, {t.TenantPublicId}, {t.Amount}, {t.Currency}");
        });
    }
}