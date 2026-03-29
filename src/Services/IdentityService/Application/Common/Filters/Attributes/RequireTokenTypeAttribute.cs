using SharedKernel.Common.Constants;

namespace IdentityService.Application.Common.Filters.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireTokenTypeAttribute(JwtTokenType tokenType) : Attribute
{
    public readonly JwtTokenType TokenType = tokenType;
}