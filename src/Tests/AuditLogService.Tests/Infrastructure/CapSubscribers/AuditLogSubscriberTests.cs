using AuditLogService.Application.Interfaces;
using AuditLogService.Infrastructure.CapSubscribers;
using FluentAssertions;
using Moq;
using SharedKernel.Contracts.AuditLogs;
using Xunit;

namespace AuditLogService.Tests.Infrastructure.CapSubscribers;

public class AuditLogSubscriberTests
{
    [Fact]
    public async Task HandleMembershipInvitedAsync_Should_Call_Writer()
    {
        var writerMock = new Mock<IAuditLogWriter>();

        var sut = new AuditLogSubscriber(writerMock.Object);

        var message = new AuditLogMessage
        {
            TenantPublicId = "tenant-001",
            ActionType = "Membership.Invited",
            Category = "Membership",
            TargetType = "Invitation",
            TargetPublicId = "invitation-001",
            TargetDisplay = "foo@test.com"
        };

        await sut.HandleMembershipEventsAsync(message, CancellationToken.None);

        writerMock.Verify(
            x => x.WriteAsync(
                It.Is<AuditLogMessage>(m =>
                    m.TenantPublicId == "tenant-001" &&
                    m.ActionType == "Membership.Invited"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMembershipRemovedAsync_Should_Call_Writer()
    {
        var writerMock = new Mock<IAuditLogWriter>();

        var sut = new AuditLogSubscriber(writerMock.Object);

        var message = new AuditLogMessage
        {
            TenantPublicId = "tenant-001",
            ActionType = "Membership.Removed",
            Category = "Membership",
            TargetType = "Membership",
            TargetPublicId = "membership-001",
            TargetDisplay = "foo@test.com"
        };

        await sut.HandleMembershipEventsAsync(message, CancellationToken.None);

        writerMock.Verify(
            x => x.WriteAsync(
                It.Is<AuditLogMessage>(m => m.ActionType == "Membership.Removed"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}