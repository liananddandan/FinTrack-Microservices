using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult<UserLoginResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<JwtTokenPair>> RefreshTokenAsync(
        string userPublicId,
        string tenantPublicId,
        string jwtVersion,
        string userRoleInTenant,
        CancellationToken cancellationToken = default);
}