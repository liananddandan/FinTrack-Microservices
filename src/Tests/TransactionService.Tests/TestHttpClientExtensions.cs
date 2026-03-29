namespace TransactionService.Tests;

public static class TestHttpClientExtensions
{
    public static void SetTestAuth(
        this HttpClient client,
        string role,
        Guid? userPublicId = null,
        Guid? tenantPublicId = null)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TenantIdHeader);

        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserIdHeader,
            (userPublicId ?? Guid.NewGuid()).ToString());

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.TenantIdHeader,
            (tenantPublicId ?? Guid.NewGuid()).ToString());
    }
}