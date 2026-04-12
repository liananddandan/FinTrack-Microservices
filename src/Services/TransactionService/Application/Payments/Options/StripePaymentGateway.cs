namespace TransactionService.Application.Payments.Options;

public class StripePaymentOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Currency { get; set; } = "NZD";
}