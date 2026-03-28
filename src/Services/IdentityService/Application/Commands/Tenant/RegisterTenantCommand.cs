using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail,
    string AdminPassword
    ) : IRequest<ServiceResult<RegisterTenantResult>>;