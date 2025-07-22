using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class UserLoginCommandHandler(IUserAppService userService) : IRequestHandler<UserLoginCommand, ServiceResult<UserLoginResult>>
{
    public Task<ServiceResult<UserLoginResult>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        return userService.UserLoginAsync(request.Email, request.Password, cancellationToken);
    }
}