using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record FetchUserInfoCommand(string UserPublicId, string JwtVersion) : IRequest<ServiceResult<UserInfoDto>>;