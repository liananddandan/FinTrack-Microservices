using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class RegisterTenantCommandHandler(
    ITenantService tenantService
) : IRequestHandler<RegisterTenantCommand, ServiceResult<RegisterTenantDto>>
{
    public async Task<ServiceResult<RegisterTenantDto>> Handle(RegisterTenantCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantService.RegisterTenantAsync(request.TenantName,
            request.AdminName, 
            request.AdminEmail,
            request.AdminPassword, 
            request.TurnstileToken,
            cancellationToken);
    }
}