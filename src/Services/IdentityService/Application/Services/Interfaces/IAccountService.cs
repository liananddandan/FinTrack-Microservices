using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult<UserLoginResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}