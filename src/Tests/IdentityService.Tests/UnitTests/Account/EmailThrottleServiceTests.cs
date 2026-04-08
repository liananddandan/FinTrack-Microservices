using FluentAssertions;
using IdentityService.Application.Accounts.Services;
using Moq;
using StackExchange.Redis;

namespace IdentityService.Tests.UnitTests.Account;


public class EmailThrottleServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock = new();
    private readonly Mock<IDatabase> _dbMock = new();

    private readonly EmailThrottleService _sut;

    public EmailThrottleServiceTests()
    {
        _redisMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_dbMock.Object);

        _sut = new EmailThrottleService(_redisMock.Object);
    }

    [Fact]
    public async Task CheckRegistrationEmailSendAllowedAsync_Should_Return_Success_When_Count_Is_Below_Limit()
    {
        _dbMock
            .Setup(x => x.StringGetAsync(
                "email-throttle:registration:minute",
                It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"5");

        var result = await _sut.CheckRegistrationEmailSendAllowedAsync();

        result.Success.Should().BeTrue();
        result.Code.Should().Be("REGISTRATION_EMAIL_ALLOWED");
        result.Message.Should().Be("Registration verification email can be sent.");
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CheckRegistrationEmailSendAllowedAsync_Should_Return_Fail_When_Count_Reaches_Limit()
    {
        _dbMock
            .Setup(x => x.StringGetAsync(
                "email-throttle:registration:minute",
                It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"20");

        var result = await _sut.CheckRegistrationEmailSendAllowedAsync();

        result.Success.Should().BeFalse();
        result.Code.Should().Be("REGISTRATION_EMAIL_THROTTLED");
        result.Message.Should().Be("Registration succeeded, but verification email was temporarily delayed due to high traffic.");
        result.Data.Should().BeFalse();
    }
    
    [Fact]
    public async Task MarkRegistrationEmailSentAsync_Should_Increment_And_Set_Expiry_When_First_Send_In_Window()
    {
        _dbMock
            .Setup(x => x.StringIncrementAsync(
                "email-throttle:registration:minute",
                1L,
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        _dbMock
            .Setup(x => x.KeyExpireAsync(
                "email-throttle:registration:minute",
                It.IsAny<TimeSpan?>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _sut.MarkRegistrationEmailSentAsync();

        _dbMock.Verify(x => x.StringIncrementAsync(
                "email-throttle:registration:minute",
                1L,
                It.IsAny<CommandFlags>()),
            Times.Once);

        _dbMock.Verify(x => x.KeyExpireAsync(
                "email-throttle:registration:minute",
                It.Is<TimeSpan?>(t => t == TimeSpan.FromMinutes(1)),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }
}