using IdentityService.Application.Commands;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class UserLoginCommandHandler(IUserAppService userService) : IRequestHandler<UserLoginCommand, ServiceResult<UserLoginResult>>
{
    public Task<ServiceResult<UserLoginResult>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        return userService.UserLoginAsync(request.Email, request.Password, cancellationToken);
    }
}