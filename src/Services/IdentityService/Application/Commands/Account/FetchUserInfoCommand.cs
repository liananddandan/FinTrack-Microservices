using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Account;

public record FetchUserInfoCommand(string UserPublicId) : IRequest<ServiceResult<CurrentUserInfoResult>>;