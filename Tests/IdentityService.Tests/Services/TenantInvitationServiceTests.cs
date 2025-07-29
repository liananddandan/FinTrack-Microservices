using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Services;

public class TenantInvitationServiceTests
{
    [Theory, AutoMoqData]
    public async Task AddTenantInvitationAsync_ShouldOnlyInvokeEachFunctionOnce(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        TenantInvitation tenantInvitation,
        TenantInvitationService sut)
    {
        // Act
        var result = await sut.AddTenantInvitationAsync(tenantInvitation, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordAddSuccess);
        tenantInvitationRepoMock.Verify(tir => tir.AddAsync(tenantInvitation), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GetTenantInvitationByPublicIdAsync_ShouldReturnFail_WhenPublicIdIsInvalid(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        string publicId,
        TenantInvitationService sut)
    {
        // Act
        var result = await sut.GetTenantInvitationByPublicIdAsync(publicId, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationPublicIdInvalid);
        tenantInvitationRepoMock.Verify(tir => tir.FindByPublicIdAsync(It.IsAny<Guid>()), Times.Never);
    }
    
    [Theory, AutoMoqData]
    public async Task GetTenantInvitationByPublicIdAsync_ShouldReturnFail_WhenRecordNotExist(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        Guid publicId,
        TenantInvitationService sut)
    {
        // Arrange
        tenantInvitationRepoMock
            .Setup(tir => tir.FindByPublicIdAsync(publicId))
            .ReturnsAsync((TenantInvitation?)null);
        
        // Act
        var result = await sut.GetTenantInvitationByPublicIdAsync(publicId.ToString(), CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordNotFound);
        tenantInvitationRepoMock.Verify(tir => tir.FindByPublicIdAsync(It.IsAny<Guid>()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GetTenantInvitationByPublicIdAsync_ShouldReturnSuccess_WhenRecordExist(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        Guid publicId,
        TenantInvitation tenantInvitation,
        TenantInvitationService sut)
    {
        // Arrange
        tenantInvitationRepoMock
            .Setup(tir => tir.FindByPublicIdAsync(publicId))
            .ReturnsAsync(tenantInvitation);
        
        // Act
        var result = await sut.GetTenantInvitationByPublicIdAsync(publicId.ToString(), CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordFoundByPublicIdSuccess);
        tenantInvitationRepoMock.Verify(tir => tir.FindByPublicIdAsync(It.IsAny<Guid>()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GetTenantInvitationByEmailAsync_ShouldReturnFail_WhenRecordNotExist(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        string email,
        TenantInvitationService sut)
    {
        // Arrange
        tenantInvitationRepoMock
            .Setup(tir => tir.FindByEmailAsync(email))
            .ReturnsAsync((TenantInvitation?)null);
        
        // Act
        var result = await sut.GetTenantInvitationByEmailAsync(email, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordNotFound);
        tenantInvitationRepoMock.Verify(tir => tir.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GetTenantInvitationByEmailAsync_ShouldReturnSuccess_WhenRecordExist(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        string email,
        TenantInvitation tenantInvitation,
        TenantInvitationService sut)
    {
        // Arrange
        tenantInvitationRepoMock
            .Setup(tir => tir.FindByEmailAsync(email))
            .ReturnsAsync(tenantInvitation);
        
        // Act
        var result = await sut.GetTenantInvitationByEmailAsync(email, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordFoundByEmailSuccess);
        tenantInvitationRepoMock.Verify(tir => tir.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task UpdateTenantInvitationAsync_ShouldOnlyInvokeEachFunctionOnce(
        [Frozen] Mock<ITenantInvitationRepo> tenantInvitationRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        TenantInvitation tenantInvitation,
        TenantInvitationService sut)
    { 
        // Act
        var result = await sut.UpdateTenantInvitationAsync(tenantInvitation, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRecordUpdateSuccess);
        tenantInvitationRepoMock.Verify(tir => tir.UpdateAsync(tenantInvitation), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}