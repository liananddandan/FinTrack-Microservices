using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record FetchUserInfoCommand(string UserPublicId) : IRequest<ServiceResult<CurrentUserInfoResult>>;