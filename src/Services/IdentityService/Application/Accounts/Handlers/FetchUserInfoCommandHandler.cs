using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

public class FetchUserInfoCommandHandler(IUserAppService userAppService) : IRequestHandler<FetchUserInfoCommand, ServiceResult<CurrentUserInfoDto>>
{
    public Task<ServiceResult<CurrentUserInfoDto>> Handle(FetchUserInfoCommand request, CancellationToken cancellationToken)
    {
        return userAppService.GetUserInfoAsync(request.UserPublicId, cancellationToken);
    }
}