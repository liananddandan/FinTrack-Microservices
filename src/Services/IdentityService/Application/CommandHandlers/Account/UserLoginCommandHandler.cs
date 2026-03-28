using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.Account;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Account;

public class UserLoginCommandHandler(IAccountService accountService) : IRequestHandler<UserLoginCommand, ServiceResult<UserLoginResult>>
{
    public async Task<ServiceResult<UserLoginResult>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        return await accountService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}