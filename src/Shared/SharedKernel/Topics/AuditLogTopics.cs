namespace SharedKernel.Topics;

public static class AuditLogTopics
{
    public const string MembershipInvited = "audit.membership.invited";
    public const string MembershipRemoved = "audit.membership.removed";
    public const string MembershipRoleChanged = "audit.membership.role-changed";
    public const string MembershipAccepted = "audit.membership.accepted";
    public const string MembershipInvitationResent = "audit.membership.invitation-resent";

    public const string TransactionCreated = "audit.transaction.created";
    public const string TransactionStatusChanged = "audit.transaction.status-changed";
}