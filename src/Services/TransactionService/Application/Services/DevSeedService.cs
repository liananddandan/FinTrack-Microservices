using TransactionService.Application.Common.DTOs;
using TransactionService.Application.DTOs;
using TransactionService.Application.Services.Interfaces;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

namespace TransactionService.Application.Services;

public class DevSeedService(
    ITransactionService transactionService,
    ITransactionRepo transactionRepo) : IDevSeedService
{
    private const string DonationTitle = "Demo Donation";
    private const string DraftProcurementTitle = "Demo Procurement Draft";
    private const string SubmittedProcurementTitle = "Demo Procurement Submitted";
    private const string ApprovedProcurementTitle = "Demo Procurement Approved";

    public async Task<DevTransactionSeedResult> SeedTransactionsAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.TenantPublicId, out var tenantPublicId))
        {
            throw new InvalidOperationException("TenantPublicId is invalid.");
        }

        if (!Guid.TryParse(request.MemberUserPublicId, out _))
        {
            throw new InvalidOperationException("MemberUserPublicId is invalid.");
        }

        if (!Guid.TryParse(request.AdminUserPublicId, out _))
        {
            throw new InvalidOperationException("AdminUserPublicId is invalid.");
        }

        var transactionPublicIds = new List<string>
        {
            (await EnsureDonationAsync(request, tenantPublicId, cancellationToken)).ToString(),
            (await EnsureProcurementAsync(request, tenantPublicId, DraftProcurementTitle, TransactionStatus.Draft, cancellationToken)).ToString(),
            (await EnsureProcurementAsync(request, tenantPublicId, SubmittedProcurementTitle, TransactionStatus.Submitted, cancellationToken)).ToString(),
            (await EnsureProcurementAsync(request, tenantPublicId, ApprovedProcurementTitle, TransactionStatus.Approved, cancellationToken)).ToString()
        };

        return new DevTransactionSeedResult(
            DonationCount: 1,
            ProcurementCount: 3,
            CreatedTransactionPublicIds: transactionPublicIds);
    }

    private async Task<Guid> EnsureDonationAsync(
        DevTransactionSeedRequest request,
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        var existing = await transactionRepo.GetByTenantAndTitleAsync(
            tenantPublicId,
            DonationTitle,
            cancellationToken);

        if (existing is not null)
        {
            return existing.PublicId;
        }

        var createResult = await transactionService.CreateDonationAsync(
            request.TenantPublicId,
            request.MemberUserPublicId,
            DonationTitle,
            "Demo successful donation",
            1000m,
            "NZD",
            cancellationToken);

        if (!createResult.Success || createResult.Data is null)
        {
            throw new InvalidOperationException(createResult.Message ?? "Failed to create demo donation.");
        }

        return Guid.Parse(createResult.Data.TransactionPublicId);
    }

    private async Task<Guid> EnsureProcurementAsync(
        DevTransactionSeedRequest request,
        Guid tenantPublicId,
        string title,
        TransactionStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var existing = await transactionRepo.GetByTenantAndTitleAsync(
            tenantPublicId,
            title,
            cancellationToken);

        if (existing is not null)
        {
            return existing.PublicId;
        }

        var createResult = await transactionService.CreateProcurementAsync(
            request.TenantPublicId,
            request.MemberUserPublicId,
            new CreateProcurementRequest
            {
                Title = title,
                Description = $"{title} description",
                Amount = 120m,
                Currency = "NZD"
            },
            cancellationToken);

        if (!createResult.Success || createResult.Data is null)
        {
            throw new InvalidOperationException(createResult.Message ?? "Failed to create demo procurement.");
        }

        var transactionPublicId = createResult.Data.TransactionPublicId;

        if (targetStatus == TransactionStatus.Draft)
        {
            return Guid.Parse(transactionPublicId);
        }

        var submitResult = await transactionService.SubmitProcurementAsync(
            request.TenantPublicId,
            request.MemberUserPublicId,
            transactionPublicId,
            cancellationToken);

        if (!submitResult.Success)
        {
            throw new InvalidOperationException(submitResult.Message ?? "Failed to submit demo procurement.");
        }

        if (targetStatus == TransactionStatus.Submitted)
        {
            return Guid.Parse(transactionPublicId);
        }

        var approveResult = await transactionService.ApproveProcurementAsync(
            request.TenantPublicId,
            "Admin",
            transactionPublicId,
            cancellationToken);

        if (!approveResult.Success)
        {
            throw new InvalidOperationException(approveResult.Message ?? "Failed to approve demo procurement.");
        }

        return Guid.Parse(transactionPublicId);
    }
}
