using System.Net.Http.Json;
using System.Text.Json;
using GatewayService.DTOs;
using GatewayService.Services.Interfaces;
using SharedKernel.Common.DTOs;

namespace GatewayService.Services;

public class DevSeedOrchestrator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IDevSeedOrchestrator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DevSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var identityBaseAddress = GetClusterAddress("identityCluster");
        var transactionBaseAddress = GetClusterAddress("transactionCluster");

        var client = httpClientFactory.CreateClient();

        var identityResponse = await client.PostAsJsonAsync(
            CombineUrl(identityBaseAddress, "api/dev/seed/identity"),
            new { },
            cancellationToken);

        var identityPayload = await ReadApiResponseAsync<DevIdentitySeedResult>(
            identityResponse,
            cancellationToken);

        if (!identityResponse.IsSuccessStatusCode || identityPayload?.Data is null)
        {
            throw new InvalidOperationException(
                $"Identity seed failed: {identityPayload?.Message ?? identityResponse.ReasonPhrase}");
        }

        var identitySeedData = identityPayload.Data;

        var transactionResponse = await client.PostAsJsonAsync(
            CombineUrl(transactionBaseAddress, "api/dev/seed/transactions"),
            new DevTransactionSeedRequest
            {
                TenantPublicId = identitySeedData.TenantPublicId,
                TenantName = identitySeedData.TenantName,
                AdminUserPublicId = identitySeedData.AdminUserPublicId,
                MemberUserPublicId = identitySeedData.MemberUserPublicId
            },
            cancellationToken);

        var transactionPayload = await ReadApiResponseAsync<DevTransactionSeedResult>(
            transactionResponse,
            cancellationToken);

        if (!transactionResponse.IsSuccessStatusCode || transactionPayload?.Data is null)
        {
            throw new InvalidOperationException(
                $"Transaction seed failed: {transactionPayload?.Message ?? transactionResponse.ReasonPhrase}");
        }

        var transactionSeedData = transactionPayload.Data;

        return new DevSeedResult(
            identitySeedData.TenantPublicId,
            identitySeedData.TenantName,
            identitySeedData.AdminEmail,
            identitySeedData.AdminPassword,
            identitySeedData.MemberEmail,
            identitySeedData.MemberPassword,
            transactionSeedData.DonationCount,
            transactionSeedData.ProcurementCount);
    }

    private string GetClusterAddress(string clusterId)
    {
        var destinations = configuration.GetSection($"ReverseProxy:Clusters:{clusterId}:Destinations").GetChildren();
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
