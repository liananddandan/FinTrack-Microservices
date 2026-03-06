using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record FetchUserInfoCommand(string UserPublicId) : IRequest<ServiceResult<UserInfoDto>>;