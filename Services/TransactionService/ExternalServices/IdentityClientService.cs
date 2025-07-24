using SharedKernel.Common.DTOs;
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
        return await response.Content.ReadFromJsonAsync<UserInfoDto>();
    }
}