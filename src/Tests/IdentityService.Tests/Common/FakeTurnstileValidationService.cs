using IdentityService.Application.Common.Abstractions;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Common;


public class FakeTurnstileValidationService : ITurnstileValidationService
{
    public Task<ServiceResult<bool>> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResult<bool>.Ok(
            true,
            "TURNSTILE_VERIFY_SUCCESS",
            "Verification passed."));
    }
}