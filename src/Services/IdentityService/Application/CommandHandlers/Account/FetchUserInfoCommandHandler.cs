using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Account;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Account;

public class FetchUserInfoCommandHandler(IUserAppService userAppService) : IRequestHandler<FetchUserInfoCommand, ServiceResult<CurrentUserInfoResult>>
{
    public Task<ServiceResult<CurrentUserInfoResult>> Handle(FetchUserInfoCommand request, CancellationToken cancellationToken)
    {
        return userAppService.GetUserInfoAsync(request.UserPublicId, cancellationToken);
    }
}