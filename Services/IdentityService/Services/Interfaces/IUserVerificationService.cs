using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface IUserVerificationService
{
    Task<ServiceResult<string>> GenerateTokenAsync(ApplicationUser user, TokenPurpose purpose, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ValidateTokenAsync(ApplicationUser user, string token, TokenPurpose purpose,CancellationToken cancellationToken = default);
}