namespace SharedKernel.Events;

public record TenantInvitationEmailRequestedEvent(
    string ToEmail,
    string TenantName,
    string InvitationLink,
    string Role
);