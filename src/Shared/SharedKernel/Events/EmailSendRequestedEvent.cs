namespace SharedKernel.Events;


public class EmailSendRequestedEvent
{
    public string To { get; set; } = default!;
    public string? ToName { get; set; }

    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;

    public bool IsHtml { get; set; }
}