using NotificationService.Handlers;
using NotificationService.Services;
using Moq;
using SharedKernel.Events;

namespace NotificationService.Tests.Handlers;

public class EmailSendEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_InvokesEmailServiceSend()
    {
        var mockEmailService = new Mock<IEmailService>();
        var handler = new EmailSendEventHandler(mockEmailService.Object);
        var emailEvent = new EmailSendRequestedEvent
        {
            From = "from@email.com",
            To = "to@test.com",
            ToName = "ToName",
            Subject = "Test",
            Body = "Hello",
            IsHtml = false
        };
        
        await handler.HandleEmailSendAsync(emailEvent);
        
        mockEmailService.Verify(s => s.SendEmailAsync(emailEvent), Times.Once);
    }
}