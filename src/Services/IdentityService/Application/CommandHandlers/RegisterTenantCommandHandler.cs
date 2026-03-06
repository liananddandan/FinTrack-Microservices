using IdentityService.Application.Commands;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class RegisterTenantCommandHandler(
    ITenantService tenantService
) : IRequestHandler<RegisterTenantCommand, ServiceResult<RegisterTenantResult>>
{
    public async Task<ServiceResult<RegisterTenantResult>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        return await tenantService.RegisterTenantAsync(request.TenantName,
            request.AdminName, request.AdminEmail, request.AdminPassword, cancellationToken);
    }
    
}