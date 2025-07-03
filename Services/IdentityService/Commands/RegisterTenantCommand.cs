using IdentityService.Common.Results;
using IdentityService.DTOs;
using MediatR;

namespace IdentityService.Commands;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail
    ) : IRequest<ServiceResult<RegisterTenantResult>>;