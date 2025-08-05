using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.ExternalServices.Interfaces;

namespace TransactionService.ExternalServices;

public class IdentityClientService : IIdentityClientService
{
    private readonly HttpClient _httpClient;
    public IdentityClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<UserInfoDto?> GetUserInfoAsync(string userPublicId)
    {
        var response = await _httpClient.GetAsync($"api/internal/account/{userPublicId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        var serviceResult = await response.Content.ReadFromJsonAsync<ServiceResult<UserInfoDto>>();
        return serviceResult?.Data;
    }
}