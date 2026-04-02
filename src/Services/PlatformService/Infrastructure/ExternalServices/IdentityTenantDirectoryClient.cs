using PlatformService.Application.Common.Options;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;
using Microsoft.Extensions.Options;
using SharedKernel.Common.DTOs;

namespace PlatformService.Infrastructure.ExternalServices;

public class IdentityTenantDirectoryClient(
    HttpClient httpClient,
    IOptions<IdentityServiceOptions> options)
    : IIdentityTenantDirectoryClient
{
    private readonly IdentityServiceOptions _options = options.Value;

    public async Task<IReadOnlyList<TenantSummaryDto>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/internal/tenants");
        request.Headers.Add("X-Internal-Key", _options.InternalApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantSummaryDto>>>(
                cancellationToken: cancellationToken);

        return apiResponse?.Data ?? [];
    }
}