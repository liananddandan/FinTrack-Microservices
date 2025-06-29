using IdentityService.DTOs;
using MediatR;

namespace IdentityService.Commands;

public record RegisterTenantCommand(
    string TenantName,
    string AdminName,
    string AdminEmail
    ) : IRequest<RegisterTenantResult>;