using System.Security.Cryptography;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

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