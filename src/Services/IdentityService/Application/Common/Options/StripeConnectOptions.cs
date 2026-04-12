namespace IdentityService.Application.Common.Options;

public class StripeConnectOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Country { get; set; } = "NZ";
    public string OnboardingRefreshUrl { get; set; } = string.Empty;
    public string OnboardingReturnUrl { get; set; } = string.Empty;
}