using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

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