using IdentityService.Application.Accounts.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Abstractions;

public interface IEmailVerificationService
{
    Task<ServiceResult<CreateEmailVerificationTokenResult>> CreateTokenAsync(
        long userId,
        string? createdByIp = null,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> VerifyTokenAsync(
        string rawToken,
        CancellationToken cancellationToken = default);
}