using Moq;
using NotificationService.Handlers;
using NotificationService.Services;
using SharedKernel.Events;

namespace NotificationService.Tests.Handlers;

public class TenantInvitationEmailEventHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock = new();

    private readonly TenantInvitationEmailEventHandler _sut;

    public TenantInvitationEmailEventHandlerTests()
    {
        _sut = new TenantInvitationEmailEventHandler(_emailServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Send_Email_When_Event_Received()
    {
        var evt = new TenantInvitationEmailRequestedEvent(
            "user@test.com",
            "FinTrack",
            "http://localhost:5174/invitations/accept?token=abc",
            "Member");

        await _sut.HandleAsync(evt);

        _emailServiceMock.Verify(
            x => x.SendEmailAsync(It.Is<EmailSendRequestedEvent>(e =>
                e.To == "user@test.com" &&
                e.Subject.Contains("FinTrack"))),
            Times.Once);
    }
}