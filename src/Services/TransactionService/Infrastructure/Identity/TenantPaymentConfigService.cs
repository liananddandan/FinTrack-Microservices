using Microsoft.Extensions.Options;
using SharedKernel.Common.DTOs;
using TransactionService.Application.Common.Options;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Infrastructure.Identity;

public class TenantPaymentConfigService(
    HttpClient httpClient,
    IOptions<IdentityServiceOptions> options,
    ILogger<TenantPaymentConfigService> logger)
    : ITenantPaymentConfigService
{
    private readonly IdentityServiceOptions _options = options.Value;

    public async Task<string?> GetStripeConnectedAccountIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            logger.LogError("IdentityService BaseUrl is not configured.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            logger.LogError("IdentityService InternalApiKey is not configured.");
            return null;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl.TrimEnd('/')}/internal/tenants/{tenantPublicId}/stripe-connect/status");

        request.Headers.Add("X-Internal-Key", _options.InternalApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Failed to fetch tenant stripe connect status from IdentityService. StatusCode: {StatusCode}, TenantPublicId: {TenantPublicId}",
                response.StatusCode,
                tenantPublicId);

            return null;
        }

        var result =
            await response.Content.ReadFromJsonAsync<ApiResponse<TenantStripeConnectStatusDto>>(cancellationToken: cancellationToken);

        return result?.Data?.ConnectedAccountId;
    }
}