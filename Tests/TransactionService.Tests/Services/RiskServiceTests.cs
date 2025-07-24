using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SharedKernel.Common.DTOs;
using TransactionService.Common.Status;
using TransactionService.ExternalServices.Interfaces;
using TransactionService.Services;
using TransactionService.Services.Interfaces;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Services;

public class RiskServiceTests
{
    [Theory, AutoMoqData]
    public async Task CheckRiskAsync_ShouldReturnNotFound_WhenUserNotExist(
        [Frozen] Mock<IIdentityClientService> identityClientServiceMock,
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        RiskService sut)
    {
        // arrange
        identityClientServiceMock.Setup(ics => ics.GetUserInfoAsync(userPublicId))
            .ReturnsAsync((UserInfoDto?)null);
        
        // act
        var result = await sut.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency);
        
        // assert
        result.Should().Be(RiskStatus.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task CheckRiskAsync_ShouldReturnNotFound_WhenTenantNotExist(
        [Frozen] Mock<IIdentityClientService> identityClientServiceMock,
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        UserInfoDto userInfo,
        RiskService sut)
    {
        // arrange
        userInfo.tenantInfoDto = null;
        identityClientServiceMock.Setup(ics => ics.GetUserInfoAsync(userPublicId))
            .ReturnsAsync(userInfo);
        
        // act
        var result = await sut.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency);
        
        // assert
        result.Should().Be(RiskStatus.TenantNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task CheckRiskAsync_ShouldReturnNotFound_WhenTenantNotMatch(
        [Frozen] Mock<IIdentityClientService> identityClientServiceMock,
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        UserInfoDto userInfo,
        RiskService sut)
    {
        // arrange
        identityClientServiceMock.Setup(ics => ics.GetUserInfoAsync(userPublicId))
            .ReturnsAsync(userInfo);
        
        // act
        var result = await sut.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency);
        
        // assert
        result.Should().Be(RiskStatus.TenantNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task CheckRiskAsync_ShouldReturnPass_WhenEveryThingExist(
        [Frozen] Mock<IIdentityClientService> identityClientServiceMock,
        string tenantPublicId, string userPublicId, decimal amount, string currency,
        UserInfoDto userInfo,
        RiskService sut)
    {
        // arrange
        userInfo.tenantInfoDto.TenantPublicId = tenantPublicId;
        identityClientServiceMock.Setup(ics => ics.GetUserInfoAsync(userPublicId))
            .ReturnsAsync(userInfo);
        
        // act
        var result = await sut.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency);
        
        // assert
        result.Should().Be(RiskStatus.Pass);
    }
}