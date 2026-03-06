using IdentityService.Application.Commands;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class FetchUserInfoCommandHandler(IUserAppService userAppService) : IRequestHandler<FetchUserInfoCommand, ServiceResult<CurrentUserInfoResult>>
{
    public Task<ServiceResult<CurrentUserInfoResult>> Handle(FetchUserInfoCommand request, CancellationToken cancellationToken)
    {
        return userAppService.GetUserInfoAsync(request.UserPublicId, cancellationToken);
    }
}