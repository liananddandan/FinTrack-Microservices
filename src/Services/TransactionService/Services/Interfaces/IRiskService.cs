using TransactionService.Common.Status;

namespace TransactionService.Services.Interfaces;

public interface IRiskService
{
    public Task<RiskStatus> CheckRiskAsync(string tenantPublicId, string userPublicId, decimal amount, string currency);
}