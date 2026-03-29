namespace SharedKernel.Topics;

public static class AuditLogTopics
{
    // Membership
    public const string MembershipInvited = "audit.membership.invited";
    public const string MembershipRemoved = "audit.membership.removed";
    public const string MembershipRoleChanged = "audit.membership.role-changed";
    public const string MembershipAccepted = "audit.membership.accepted";
    public const string MembershipInvitationResent = "audit.membership.invitation-resent";

    // Transaction
    public const string TransactionCreated = "audit.transaction.created";
    public const string TransactionStatusChanged = "audit.transaction.status-changed";

    // Order
    public const string OrderCreated = "audit.order.created";
    public const string OrderCancelled = "audit.order.cancelled";
    public const string OrderStatusChanged = "audit.order.status-changed";
    public const string OrderPaymentStatusChanged = "audit.order.payment-status-changed";

    // Menu
    // menu item
    public const string MenuItemCreated = "audit.menu-item.created";
    public const string MenuItemUpdated = "audit.menu-item.updated";
    public const string MenuItemDeleted = "audit.menu-item.deleted";
    public const string MenuItemPriceChanged = "audit.menu-item.price-changed";

    // menu category
    public const string MenuCategoryCreated = "audit.menu-category.created";
    public const string MenuCategoryUpdated = "audit.menu-category.updated";
    public const string MenuCategoryDeleted = "audit.menu-category.deleted";
    public const string MenuCategorySortChanged = "audit.menu-category.sort-changed";

    // Tenant
    public const string TenantUpdated = "audit.tenant.updated";

    // User / Auth
    public const string UserLoggedIn = "audit.user.logged-in";
    public const string UserLoginFailed = "audit.user.login-failed";
}