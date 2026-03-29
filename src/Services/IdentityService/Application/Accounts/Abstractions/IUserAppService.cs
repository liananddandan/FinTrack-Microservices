using IdentityService.Application.Common.DTOs;
using IdentityService.Domain.Entities;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Abstractions;

public interface IUserAppService
{
    Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id);

    Task<ServiceResult<ConfirmAccountEmailDto>> ConfirmAccountEmailAsync(string userPublicId, string token,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserLoginDto>> UserLoginAsync(string email, string password,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> SetUserPasswordAsync(string userPublicId,
        string oldPassword, string newPassword, bool reset, CancellationToken cancellationToken = default);

    Task<ServiceResult<JwtTokenPair>> RefreshUserTokenPairAsync(string userPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CurrentUserInfoDto>> GetUserInfoAsync(string userPublicId,
        CancellationToken cancellationToken = default);
}