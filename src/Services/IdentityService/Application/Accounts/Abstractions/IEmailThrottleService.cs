using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Abstractions;

public interface IEmailThrottleService
{
    Task<ServiceResult<bool>> CheckRegistrationEmailSendAllowedAsync(
        CancellationToken cancellationToken = default);

    Task MarkRegistrationEmailSentAsync(
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> CheckVerificationResendAllowedAsync(
        long userId,
        string email,
        CancellationToken cancellationToken = default);

    Task MarkVerificationResendSentAsync(
        long userId,
        string email,
        CancellationToken cancellationToken = default);
}