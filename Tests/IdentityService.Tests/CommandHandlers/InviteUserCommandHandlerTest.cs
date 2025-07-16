using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class InviteUserCommandHandlerTest
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResponse_WhenReceivedCommand(
        [Frozen] Mock<ITenantService> tenantServiceMock,
        InviteUserCommand inviteUserCommand,
        InviteUserCommandHandler sut)
    {
        // Arrange
        tenantServiceMock.Setup(ts => ts.InviteUserForTenantAsync(
            inviteUserCommand.AdminUserPublicId, inviteUserCommand.AdminJwtVersion,
            inviteUserCommand.TenantPublicid, inviteUserCommand.AdminRoleInTenant,
            inviteUserCommand.Emails, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Ok(true, "invite", "invite"));
        
        // Act
        var result = await sut.Handle(inviteUserCommand, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }
}