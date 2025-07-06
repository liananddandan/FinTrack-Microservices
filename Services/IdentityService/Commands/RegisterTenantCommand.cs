using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail
    ) : IRequest<ServiceResult<RegisterTenantResult>>;