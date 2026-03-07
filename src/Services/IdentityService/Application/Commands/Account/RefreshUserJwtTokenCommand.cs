using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Account;


public record RefreshUserJwtTokenCommand(
    string UserPublicId,
    string TenantPublicId,
    string JwtVersion,
    string UserRoleInTenant
) : IRequest<ServiceResult<JwtTokenPair>>;