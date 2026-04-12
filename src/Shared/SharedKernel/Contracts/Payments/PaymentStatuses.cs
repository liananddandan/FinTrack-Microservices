namespace SharedKernel.Contracts.Payments;

public static class PaymentStatuses
{
    public const string NotStarted = "NotStarted";
    public const string Pending = "Pending";
    public const string RequiresAction = "RequiresAction";
    public const string Processing = "Processing";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
}