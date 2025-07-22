using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class RegisterTenantCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_FromService(
        [Frozen] Mock<IUserAppService> userAppService,
        ConfirmAccountEmailCommandHandler sut,
        ConfirmAccountEmailCommand command,
        ConfirmAccountEmailResult result)
    {
        userAppService.Setup(x => x.ConfirmAccountEmailAsync(
                command.UserPublicId, command.Token, CancellationToken.None))
            .ReturnsAsync(ServiceResult<ConfirmAccountEmailResult>.Ok(result, "Confirm Account", "Confirm Account"));

        var actual = await sut.Handle(command, CancellationToken.None);

        actual.Data.Should().Be(result);
    }
}