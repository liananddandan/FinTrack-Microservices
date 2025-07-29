using TransactionService.Common.Status;
using TransactionService.ExternalServices.Interfaces;
using TransactionService.Services.Interfaces;

namespace TransactionService.Services;

public class RiskService(IIdentityClientService identityClientService) : IRiskService
{
    public async Task<RiskStatus> CheckRiskAsync(string tenantPublicId, string userPublicId, decimal amount, string currency)
    {
        var userInfo = await identityClientService.GetUserInfoAsync(userPublicId);
        if (userInfo == null)
        {
            return RiskStatus.UserNotFound;
        }

        if (userInfo.tenantInfoDto == null 
            || userInfo.tenantInfoDto.TenantPublicId != tenantPublicId)
        {
            return RiskStatus.TenantNotFound;
        }
        
        return RiskStatus.Pass;
    }
}