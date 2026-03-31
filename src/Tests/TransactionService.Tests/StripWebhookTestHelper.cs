namespace TransactionService.Tests;

using Stripe;

public static class StripeWebhookTestHelper
{
    public static string CreateSignatureHeader(string payload, string secret, long? timestamp = null)
    {
        var ts = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = EventUtility.ComputeSignature(secret, ts.ToString(), payload);
        return $"t={ts},v1={signature}";
    }
}