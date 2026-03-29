using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Commands;

public record FetchUserInfoCommand(string UserPublicId) : IRequest<ServiceResult<CurrentUserInfoDto>>;