namespace IdentityService.Application.Common.Options;

public class TurnstileOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string VerifyUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
}