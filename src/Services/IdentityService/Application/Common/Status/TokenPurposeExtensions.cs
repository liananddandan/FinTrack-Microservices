namespace IdentityService.Application.Common.Status;

public static class TokenPurposeExtensions
{
    public static string ToIdentityString(this TokenPurpose tokenPurpose)
    {
        return tokenPurpose.ToString();
    }
}