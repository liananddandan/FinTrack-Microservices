using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class GetUsersInTenantCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenReceiveCommand(
        [Frozen] Mock<ITenantService> tenantService,
        List<UserInfoDto> users,
        GetUsersInTenantCommand command,
        GetUsersInTenantCommandHandler sut)
    {
        // Arrange
        tenantService.Setup(ts => ts.GetUsersForTenantAsync(command.AdminPublicId,
            command.TenantPublicId, command.AdminRoleInTenant, CancellationToken.None))
            .ReturnsAsync(ServiceResult<IEnumerable<UserInfoDto>>.Ok(users, "Get users for tenant", "Get users for tenant"));
        
        // Act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(users);
    }
}