using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Common.Results;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;

namespace IdentityService.Tests.CommandHandlers;

public class RegisterTenantCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_FromService(
        [Frozen] Mock<ITenantService> tenantServiceMock,
        RegisterTenantCommandHandler sut,
        RegisterTenantCommand command,
        RegisterTenantResult result)
    {
        tenantServiceMock.Setup(x => x.RegisterTenantAsync(
                command.TenantName, command.AdminName, command.AdminEmail, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<RegisterTenantResult>.Ok(result, "registerTenant", "registerTenant"));

        var actual = await sut.Handle(command, CancellationToken.None);

        actual.Data.Should().Be(result);
    }
}