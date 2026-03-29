using System.Text.Json;
using GatewayService.Application.Dev.Abstractions;
using GatewayService.Application.Dev.Dtos;
using SharedKernel.Common.DTOs;
using SharedKernel.Contracts.Dev;

namespace GatewayService.Application.Dev.Services;

public class DevSeedOrchestrator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IDevSeedOrchestrator
{
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly IReadOnlyList<DevTenantSeedSpec> SeedSpecs =
    [
        new(
            TenantName: "Auckland Coffee",
            AdminEmail: "admin@auckland-coffee.local",
            AdminPassword: "Admin123!",
            MemberEmail: "staff@auckland-coffee.local",
            MemberPassword: "Member123!",
            Template: "coffee"),

        new(
            TenantName: "Tokyo Sushi Bar",
            AdminEmail: "admin@tokyo-sushi.local",
            AdminPassword: "Admin123!",
            MemberEmail: "staff@tokyo-sushi.local",
            MemberPassword: "Member123!",
            Template: "sushi")
    ];

    public async Task<DevSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var identityBaseAddress = GetClusterAddress("identityCluster");
        var transactionBaseAddress = GetClusterAddress("transactionCluster");

        var client = httpClientFactory.CreateClient();
        var results = new List<DevSeedTenantResult>();

        foreach (var spec in SeedSpecs)
        {
            var identitySeed = await SeedIdentityAsync(
                client,
                identityBaseAddress,
                spec,
                cancellationToken);

            var transactionSeed = await SeedTransactionDataAsync(
                client,
                transactionBaseAddress,
                identitySeed,
                spec.Template,
                cancellationToken);

            results.Add(new DevSeedTenantResult(
                TenantPublicId: identitySeed.TenantPublicId,
                TenantName: identitySeed.TenantName,
                AdminEmail: identitySeed.AdminEmail,
                AdminPassword: identitySeed.AdminPassword,
                MemberEmail: identitySeed.MemberEmail,
                MemberPassword: identitySeed.MemberPassword,
                CategoryCount: transactionSeed.CategoryCount,
                ProductCount: transactionSeed.ProductCount,
                OrderCount: transactionSeed.OrderCount));
        }

        return new DevSeedResult(results);
    }

    private async Task<DevIdentitySeedResult> SeedIdentityAsync(
        HttpClient client,
        string identityBaseAddress,
        DevTenantSeedSpec spec,
        CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(
            CombineUrl(identityBaseAddress, "api/dev/seed/identity"),
            new DevIdentitySeedRequest
            {
                TenantName = spec.TenantName,
                AdminEmail = spec.AdminEmail,
                AdminPassword = spec.AdminPassword,
                MemberEmail = spec.MemberEmail,
                MemberPassword = spec.MemberPassword
            },
            cancellationToken);

        var payload = await ReadApiResponseAsync<DevIdentitySeedResult>(response, cancellationToken);

        if (!response.IsSuccessStatusCode || payload?.Data is null)
        {
            throw new InvalidOperationException(
                $"Identity seed failed for '{spec.TenantName}': {payload?.Message ?? response.ReasonPhrase}");
        }

        return payload.Data;
    }

    private async Task<DevTransactionSeedResult> SeedTransactionDataAsync(
        HttpClient client,
        string transactionBaseAddress,
        DevIdentitySeedResult identitySeed,
        string template,
        CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(
            CombineUrl(transactionBaseAddress, "api/dev/seed/menu-and-orders"),
            new DevTransactionSeedRequest
            {
                TenantPublicId = identitySeed.TenantPublicId,
                TenantName = identitySeed.TenantName,
                AdminUserPublicId = identitySeed.AdminUserPublicId,
                MemberUserPublicId = identitySeed.MemberUserPublicId,
                AdminUserEmail = identitySeed.AdminEmail,
                MemberUserEmail = identitySeed.MemberEmail,
                Template = template
            },
            cancellationToken);
        
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Transaction seed failed for '{identitySeed.TenantName}'. " +
                $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                $"Body: {raw}");
        }

        var payload = string.IsNullOrWhiteSpace(raw)
            ? null
            : JsonSerializer.Deserialize<ApiResponse<DevTransactionSeedResult>>(raw, JsonOptions);

        if (payload?.Data is null)
        {
            throw new InvalidOperationException(
                $"Transaction seed failed for '{identitySeed.TenantName}'. " +
                $"Body: {raw}");
        }

        return payload.Data;
    }

    private string GetClusterAddress(string clusterId)
    {
        var destinations = configuration
            .GetSection($"ReverseProxy:Clusters:{clusterId}:Destinations")
            .GetChildren();

        var address = destinations
            .Select(x => x["Address"])
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException($"Cluster '{clusterId}' destination address is not configured.");
        }

        return address;
    }

    private static string CombineUrl(string baseAddress, string relativePath)
    {
        var normalizedBase = baseAddress.TrimEnd('/');
        var normalizedPath = relativePath.TrimStart('/');
        return $"{normalizedBase}/{normalizedPath}";
    }

    private static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
    }
}