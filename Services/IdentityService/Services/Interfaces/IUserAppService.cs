using IdentityService.Common.Results;
using IdentityService.Domain.Entities;

namespace IdentityService.Services.Interfaces;

public interface IUserAppService
{
    Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id);
}