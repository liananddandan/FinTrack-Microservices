using System.Security.Cryptography;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class RegisterTenantCommandHandler(
    ITenantService tenantService
) : IRequestHandler<RegisterTenantCommand, ServiceResult<RegisterTenantResult>>
{
    public async Task<ServiceResult<RegisterTenantResult>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        return await tenantService.RegisterTenantAsync(request.TenantName,
            request.AdminName, request.AdminEmail, cancellationToken);
    }
    
}