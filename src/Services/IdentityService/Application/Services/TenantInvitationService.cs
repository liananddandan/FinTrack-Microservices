using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services;

public class TenantInvitationService(
    ITenantInvitationRepo tenantInvitationRepo,
    IUnitOfWork unitOfWork) : ITenantInvitationService
{
    public async Task<ServiceResult<bool>> AddTenantInvitationAsync(TenantInvitation invitation, CancellationToken cancellationToken)
    {
        await tenantInvitationRepo.AddAsync(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Ok(true, 
            ResultCodes.Tenant.InvitationRecordAddSuccess, 
            "Invitation Record Add Success");
    }

    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByPublicIdAsync(string publicId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<bool>> UpdateTenantInvitationAsync(TenantInvitation invitation, CancellationToken cancellationToken = default)
    {
       await tenantInvitationRepo.UpdateAsync(invitation);
       await unitOfWork.SaveChangesAsync(cancellationToken);
       return ServiceResult<bool>.Ok(true,
           ResultCodes.Tenant.InvitationRecordUpdateSuccess,
           "Invitation Record Update Success");
    }
}