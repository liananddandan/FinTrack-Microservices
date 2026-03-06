using IdentityService.Domain.Entities;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

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

    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(publicId, out Guid id))
        {
            return ServiceResult<TenantInvitation>
                .Fail(ResultCodes.Tenant.InvitationPublicIdInvalid, 
                    "Invitation Public Id Invalid"); 
        }

        var invitation = await tenantInvitationRepo.FindByPublicIdAsync(id);
        return invitation == null
            ? ServiceResult<TenantInvitation>
                .Fail(ResultCodes.Tenant.InvitationRecordNotFound,
                    "Invitation Record Not Found")
            : ServiceResult<TenantInvitation>.Ok(invitation, 
                ResultCodes.Tenant.InvitationRecordFoundByPublicIdSuccess,
                "Invitation Record Found By Public Id");
    }

    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var invitation = await tenantInvitationRepo.FindByEmailAsync(email);
        return invitation == null
            ? ServiceResult<TenantInvitation>
                .Fail(ResultCodes.Tenant.InvitationRecordNotFound,
                    "Invitation Record Not Found")
            : ServiceResult<TenantInvitation>.Ok(invitation, 
                ResultCodes.Tenant.InvitationRecordFoundByEmailSuccess,
                "Invitation Record Found By Public Id");
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