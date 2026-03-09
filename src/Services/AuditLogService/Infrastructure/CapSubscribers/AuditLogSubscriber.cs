using AuditLogService.Application.Interfaces;
using DotNetCore.CAP;
using SharedKernel.Contracts.AuditLogs;
using SharedKernel.Topics;

namespace AuditLogService.Infrastructure.CapSubscribers;

public class AuditLogSubscriber(
    IAuditLogWriter writer) : ICapSubscribe
{
    [CapSubscribe(AuditLogTopics.MembershipInvited)]
    [CapSubscribe(AuditLogTopics.MembershipRemoved)]
    [CapSubscribe(AuditLogTopics.MembershipRoleChanged)]
    [CapSubscribe(AuditLogTopics.MembershipAccepted)]
    [CapSubscribe(AuditLogTopics.MembershipInvitationResent)]
    public async Task HandleMembershipEventsAsync(
        AuditLogMessage message,
        CancellationToken cancellationToken)
    {
        await writer.WriteAsync(message, cancellationToken);
    }
}