using SharedKernel.Common.DTOs;

namespace TransactionService.ExternalServices.Interfaces;

public interface IIdentityClientService
{
    Task<UserInfoDto?> GetUserInfoAsync(string userPublicId);
}