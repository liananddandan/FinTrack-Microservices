namespace IdentityService.Api.Common.Filters.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireInternalApiKeyAttribute : Attribute
{
}