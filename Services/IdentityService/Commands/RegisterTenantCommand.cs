using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using MediatR;

namespace IdentityService.Commands;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail
    ) : IRequest<ServiceResult<RegisterTenantResult>>;