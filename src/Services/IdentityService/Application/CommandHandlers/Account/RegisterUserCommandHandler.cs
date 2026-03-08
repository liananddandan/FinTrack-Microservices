using IdentityService.Application.Commands.Account;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Account;

public class RegisterUserCommandHandler(IAccountService accountService)
    : IRequestHandler<RegisterUserCommand, ServiceResult<RegisterUserResult>>
{
    public async Task<ServiceResult<RegisterUserResult>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return await accountService.RegisterUserAsync(
            request.UserName,
            request.Email,
            request.Password,
            cancellationToken);
    }
}