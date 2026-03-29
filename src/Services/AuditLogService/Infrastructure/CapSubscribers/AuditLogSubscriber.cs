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
    
    [CapSubscribe(AuditLogTopics.MenuItemCreated)]
    [CapSubscribe(AuditLogTopics.MenuItemUpdated)]
    [CapSubscribe(AuditLogTopics.MenuItemDeleted)]
    [CapSubscribe(AuditLogTopics.MenuItemPriceChanged)]
    [CapSubscribe(AuditLogTopics.MenuCategoryCreated)]
    [CapSubscribe(AuditLogTopics.MenuCategoryUpdated)]
    [CapSubscribe(AuditLogTopics.MenuCategoryDeleted)]
    [CapSubscribe(AuditLogTopics.MenuCategorySortChanged)]
    public async Task HandleMenuEventsAsync(
        AuditLogMessage message,
        CancellationToken cancellationToken)
    {
        await writer.WriteAsync(message, cancellationToken);
    }
    
    [CapSubscribe(AuditLogTopics.OrderCreated)]
    [CapSubscribe(AuditLogTopics.OrderCancelled)]
    [CapSubscribe(AuditLogTopics.OrderStatusChanged)]
    [CapSubscribe(AuditLogTopics.OrderPaymentStatusChanged)]
    public async Task HandleOrderEventsAsync(
        AuditLogMessage message,
        CancellationToken cancellationToken)
    {
        await writer.WriteAsync(message, cancellationToken);
    }
}