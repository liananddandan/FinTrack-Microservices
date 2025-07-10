using IdentityService.Common.DTOs;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface IUserAppService
{
    Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id);
    Task<ServiceResult<ConfirmAccountEmailResult>> ConfirmAccountEmailAsync(string userPublicId, string token, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserLoginResult>> UserLoginAsync(string email, string password, CancellationToken cancellationToken = default);
}