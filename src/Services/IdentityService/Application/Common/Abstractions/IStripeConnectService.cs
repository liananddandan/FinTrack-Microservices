using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Common.Abstractions;


public interface IStripeConnectService
{
    Task<ServiceResult<CreateConnectedAccountResult>> CreateConnectedAccountAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CreateAccountOnboardingLinkResult>> CreateOnboardingLinkAsync(
        string connectedAccountId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StripeConnectedAccountStatusResult>> GetAccountStatusAsync(
        string connectedAccountId,
        CancellationToken cancellationToken = default);
}
