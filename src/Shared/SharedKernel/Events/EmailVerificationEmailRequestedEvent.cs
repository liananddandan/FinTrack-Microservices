namespace SharedKernel.Events;

public sealed class EmailVerificationEmailRequestedEvent
{
    public string ToEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string VerificationLink { get; set; } = string.Empty;
}