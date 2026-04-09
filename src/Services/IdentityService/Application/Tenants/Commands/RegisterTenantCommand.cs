using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail,
    string AdminPassword,
    string TurnstileToken
    ) : IRequest<ServiceResult<RegisterTenantDto>>;