using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record SelectTenantCommand(
    string UserPublicId,
    string TenantPublicId
) : IRequest<ServiceResult<string>>;