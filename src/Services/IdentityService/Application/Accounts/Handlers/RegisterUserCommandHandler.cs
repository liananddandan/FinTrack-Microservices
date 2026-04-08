using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

public class RegisterUserCommandHandler(IAccountService accountService)
    : IRequestHandler<RegisterUserCommand, ServiceResult<RegisterUserDto>>
{
    public async Task<ServiceResult<RegisterUserDto>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return await accountService.RegisterUserAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.TurnstileToken,
            cancellationToken);
    }
}