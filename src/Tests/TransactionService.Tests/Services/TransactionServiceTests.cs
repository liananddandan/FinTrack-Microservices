using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Common.Status;
using TransactionService.Domain.Entities;
using TransactionService.Repositories.Interfaces;
using TransactionService.Services.Interfaces;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Services;

public class TransactionServiceTests
{
    [Theory, AutoMoqData]
    public async Task CreateTransactionAsync_ShouldReturnFail_WhenRiskStatusFailed(
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        string? description,
        [Frozen] Mock<IRiskService> riskServiceMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        riskServiceMock.Setup(rs => rs.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency))
            .ReturnsAsync(RiskStatus.Reject);
        
        // act
        var result = await sut.CreateTransactionAsync(tenantPublicId, userPublicId, amount, currency, description);
        
        // assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionCreateFailed);
    }
    
    [Theory, AutoMoqData]
    public async Task CreateTransactionAsync_ShouldReturnSuccess_WhenRiskStatusPass(
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        string? description,
        [Frozen] Mock<IRiskService> riskServiceMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        riskServiceMock.Setup(rs => rs.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency))
            .ReturnsAsync(RiskStatus.Pass);
        
        // act
        var result = await sut.CreateTransactionAsync(tenantPublicId, userPublicId, amount, currency, description);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionCreateSuccess);
        result.Data.RiskStatus.Should().Be(RiskStatus.Pass);
        result.Data.TranStatus.Should().Be(TransStatus.Success);
        result.Data.Amount.Should().Be(amount);
        result.Data.Currency.Should().Be(currency);
    }

    [Theory, AutoMoqData]
    public async Task QueryUserOwnTransactionByPublicIdAsync_ShouldReturnFail_WhenTransactionNotExist(
        string tenantPublicId, string userPublicId, string transactionPublicId,
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tr => tr.GetTransactionByPublicIdAsync(transactionPublicId))
            .ReturnsAsync((Transaction?)null);
        
        // act
        var result = await sut.QueryUserOwnTransactionByPublicIdAsync(tenantPublicId, userPublicId, transactionPublicId);
        
        // assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task QueryUserOwnTransactionByPublicIdAsync_ShouldReturnFail_WhenUserPublicIdNotMatches(
        string userPublicId, string transactionPublicId,
        Transaction transaction,
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tr => tr.GetTransactionByPublicIdAsync(transactionPublicId))
            .ReturnsAsync(transaction);
        
        // act
        var result = await sut.QueryUserOwnTransactionByPublicIdAsync(transaction.TenantPublicId, userPublicId, transactionPublicId);
        
        // assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionNotBelongToCurrentUser);
    }
    
    [Theory, AutoMoqData]
    public async Task QueryUserOwnTransactionByPublicIdAsync_ShouldReturnFail_WhenTenantPublicIdNotMatches(
        string tenantPublicId, string transactionPublicId,
        Transaction transaction,
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tr => tr.GetTransactionByPublicIdAsync(transactionPublicId))
            .ReturnsAsync(transaction);
        
        // act
        var result = await sut.QueryUserOwnTransactionByPublicIdAsync(tenantPublicId, transaction.UserPublicId, transactionPublicId);
        
        // assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionNotBelongToCurrentUser);
    }
    
    [Theory, AutoMoqData]
    public async Task QueryUserOwnTransactionByPublicIdAsync_ShouldReturnSuccess_WhenAllMatches(
        string transactionPublicId,
        Transaction transaction,
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tr => tr.GetTransactionByPublicIdAsync(transactionPublicId))
            .ReturnsAsync(transaction);
        
        // act
        var result = await sut.QueryUserOwnTransactionByPublicIdAsync(transaction.TenantPublicId,
            transaction.UserPublicId, transactionPublicId);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionQuerySuccess);
        result.Data.TransactionPublicId.Should().Be(transaction.TransactionPublicId.ToString());
        result.Data.Amount.Should().Be(transaction.Amount);
        result.Data.Currency.Should().Be(transaction.Currency);
    }

    [Theory, AutoMoqData]
    public async Task QueryTransactionByPageAsync_ShouldReturnEmpty_WhenTransactionNotExist(
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        string tenantPublicId,
        string userPublicId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize, string sortBy,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tp
            => tp.GetTransactionsByPageAsync(tenantPublicId, userPublicId, startDate, endDate, page, pageSize, sortBy))
            .ReturnsAsync((new List<Transaction>(), 0));
        
        // act
        var result = await sut.QueryTransactionByPageAsync(tenantPublicId, userPublicId, startDate, endDate, page, pageSize, sortBy);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionQueryByPageSuccess);
        result.Data.transactions.Should().BeEmpty();
        result.Data.totalCount.Should().Be(0);
        result.Data.page.Should().Be(page);
        result.Data.pageSize.Should().Be(pageSize);
    }
    
    [Theory, AutoMoqData]
    public async Task QueryTransactionByPageAsync_ShouldReturnResult_WhenTransactionsExist(
        [Frozen] Mock<ITransactionRepo> transactionRepoMock,
        string tenantPublicId,
        string userPublicId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize, string sortBy,
        List<Transaction> transactions,
        TransactionService.Services.TransactionService sut)
    {
        // arrange
        transactionRepoMock.Setup(tp
                => tp.GetTransactionsByPageAsync(tenantPublicId, userPublicId, startDate, endDate, page, pageSize, sortBy))
            .ReturnsAsync((transactions, 100));
        
        // act
        var result = await sut.QueryTransactionByPageAsync(tenantPublicId, userPublicId, startDate, endDate, page, pageSize, sortBy);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionQueryByPageSuccess);
        result.Data.transactions.Should().NotBeEmpty();
        result.Data.totalCount.Should().Be(100);
        result.Data.page.Should().Be(page);
        result.Data.pageSize.Should().Be(pageSize);
    }
}