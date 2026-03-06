using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Application.CommandHandlers;
using IdentityService.Application.Commands;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class ConfirmAccountEmailCommandHandlerTests
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