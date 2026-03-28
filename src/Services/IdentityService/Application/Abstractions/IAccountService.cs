using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Abstractions;

public interface IAccountService
{
    Task<ServiceResult<UserLoginResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<JwtTokenPair>> RefreshTokenAsync(
        string userPublicId,
        string jwtVersion,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<RegisterUserResult>> RegisterUserAsync(
        string userName,
        string email,
        string password,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<string>> SelectTenantAsync(
        string userPublicId,
        string tenantPublicId,
        CancellationToken cancellationToken = default);
}