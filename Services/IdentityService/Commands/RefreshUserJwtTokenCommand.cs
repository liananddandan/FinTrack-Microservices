using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record RefreshUserJwtTokenCommand(string UserPublicId, string JwtVersion)
    : IRequest<ServiceResult<JwtTokenPair>>;