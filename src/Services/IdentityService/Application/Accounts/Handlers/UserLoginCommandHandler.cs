using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

public class UserLoginCommandHandler(IAccountService accountService) : IRequestHandler<UserLoginCommand, ServiceResult<UserLoginDto>>
{
    public async Task<ServiceResult<UserLoginDto>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        return await accountService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}