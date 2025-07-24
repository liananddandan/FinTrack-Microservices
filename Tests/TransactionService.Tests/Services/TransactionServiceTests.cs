using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Common.Status;
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
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionFailed);
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
        result.Code.Should().BeEquivalentTo(ResultCodes.Transaction.TransactionSuccess);
        result.Data.RiskStatus.Should().Be(RiskStatus.Pass);
        result.Data.TranStatus.Should().Be(TransactionStatus.Success);
        result.Data.Amount.Should().Be(amount);
        result.Data.Currency.Should().Be(currency);
    }
}