using SharedKernel.Common.Results;

namespace IdentityService.Application.Common.Abstractions;

public interface ITurnstileValidationService
{
    Task<ServiceResult<bool>> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default);
}