using IdentityService.Common.Status;

namespace IdentityService.Filters.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireTokenTypeAttribute(JwtTokenType tokenType) : Attribute
{
    public readonly JwtTokenType TokenType = tokenType;
}