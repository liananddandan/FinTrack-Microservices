using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record SelectTenantCommand(
    string UserPublicId,
    string TenantPublicId
) : IRequest<ServiceResult<string>>;