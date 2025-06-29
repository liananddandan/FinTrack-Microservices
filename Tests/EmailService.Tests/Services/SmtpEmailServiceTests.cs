using EmailService.Options;
using EmailService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SharedKernel.Events;

namespace EmailService.Tests.Services;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task SendAsync_WithValidInput_SendsEmailWithoutException()
    {
        var smtpOptions = new SmtpOptions
        {
            Host="sandbox.smtp.mailtrap.io",
            Port=2525,
            User="813ab4f5a7c0eb",
            Password="ac67007935ef36"
        };
        var optionsMock = new Mock<IOptions<SmtpOptions>>();
        optionsMock.Setup(o => o.Value).Returns(smtpOptions);
        var loggerMock = new Mock<ILogger<SmtpEmailService>>();
        
        var emailService = new SmtpEmailService(optionsMock.Object, loggerMock.Object);

        var emailEvent = new EmailSendRequestedEvent
        {
            From = "from@test.com",
            To = "to@test.com",
            ToName = "Receiver",
            Subject = "Test Subject",
            Body = "Test Body",
            IsHtml = false
        };
        
        await emailService.SendEmailAsync(emailEvent);
    }
}