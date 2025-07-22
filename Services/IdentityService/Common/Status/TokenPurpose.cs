namespace IdentityService.Common.Status;

public enum TokenPurpose
{
    EmailConfirmation,
    [Obsolete("only use for testing purposes")]
    InvalidType
}