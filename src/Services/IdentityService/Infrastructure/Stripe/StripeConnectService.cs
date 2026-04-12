using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Options;
using Microsoft.Extensions.Options;
using SharedKernel.Common.Results;
using Stripe;

namespace IdentityService.Infrastructure.Stripe;

public class StripeConnectService(
    IOptions<StripeConnectOptions> options,
    ILogger<StripeConnectService> logger)
    : IStripeConnectService
{
    private readonly StripeConnectOptions _options = options.Value;

    public async Task<ServiceResult<CreateConnectedAccountResult>> CreateConnectedAccountAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            StripeConfiguration.ApiKey = _options.SecretKey;

            var service = new AccountService();

            var account = await service.CreateAsync(
                new AccountCreateOptions
                {
                    Type = "express",
                    Country = _options.Country,
                    BusinessType = "company",
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        CardPayments = new AccountCapabilitiesCardPaymentsOptions
                        {
                            Requested = true
                        },
                        Transfers = new AccountCapabilitiesTransfersOptions
                        {
                            Requested = true
                        }
                    }
                },
                cancellationToken: cancellationToken);

            return ServiceResult<CreateConnectedAccountResult>.Ok(
                new CreateConnectedAccountResult(
                    account.Id,
                    account.ChargesEnabled,
                    account.PayoutsEnabled),
                "STRIPE_CONNECTED_ACCOUNT_CREATED",
                "Stripe connected account created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Stripe connected account.");

            return ServiceResult<CreateConnectedAccountResult>.Fail(
                "STRIPE_CONNECTED_ACCOUNT_CREATE_FAILED",
                "Failed to create Stripe connected account.");
        }
    }

    public async Task<ServiceResult<CreateAccountOnboardingLinkResult>> CreateOnboardingLinkAsync(
        string connectedAccountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            StripeConfiguration.ApiKey = _options.SecretKey;

            var service = new AccountLinkService();

            var link = await service.CreateAsync(
                new AccountLinkCreateOptions
                {
                    Account = connectedAccountId,
                    Type = "account_onboarding",
                    RefreshUrl = _options.OnboardingRefreshUrl,
                    ReturnUrl = _options.OnboardingReturnUrl
                },
                cancellationToken: cancellationToken);

            return ServiceResult<CreateAccountOnboardingLinkResult>.Ok(
                new CreateAccountOnboardingLinkResult(link.Url),
                "STRIPE_ONBOARDING_LINK_CREATED",
                "Stripe onboarding link created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create Stripe onboarding link for connected account {ConnectedAccountId}.",
                connectedAccountId);

            return ServiceResult<CreateAccountOnboardingLinkResult>.Fail(
                "STRIPE_ONBOARDING_LINK_CREATE_FAILED",
                "Failed to create Stripe onboarding link.");
        }
    }

    public async Task<ServiceResult<StripeConnectedAccountStatusResult>> GetAccountStatusAsync(
        string connectedAccountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            StripeConfiguration.ApiKey = _options.SecretKey;

            var service = new AccountService();
            var account = await service.GetAsync(
                connectedAccountId,
                cancellationToken: cancellationToken);

            return ServiceResult<StripeConnectedAccountStatusResult>.Ok(
                new StripeConnectedAccountStatusResult(
                    account.ChargesEnabled,
                    account.PayoutsEnabled),
                "STRIPE_CONNECTED_ACCOUNT_STATUS_OK",
                "Stripe connected account status loaded.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get Stripe connected account status for {ConnectedAccountId}.",
                connectedAccountId);

            return ServiceResult<StripeConnectedAccountStatusResult>.Fail(
                "STRIPE_CONNECTED_ACCOUNT_STATUS_FAILED",
                "Failed to get Stripe connected account status.");
        }
    }
}