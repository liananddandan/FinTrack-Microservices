using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record RefreshUserJwtTokenCommand(string UserPublicId) : IRequest<ServiceResult<JwtTokenPair>>;