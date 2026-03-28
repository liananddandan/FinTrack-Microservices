using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Account;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Account;

public class RefreshUserJwtTokenCommandHandler(IAccountService accountService)
    : IRequestHandler<RefreshUserJwtTokenCommand, ServiceResult<JwtTokenPair>>
{
    public async Task<ServiceResult<JwtTokenPair>> Handle(RefreshUserJwtTokenCommand request, CancellationToken cancellationToken)
    {
        return await accountService.RefreshTokenAsync(
            request.UserPublicId,
            request.JwtVersion,
            cancellationToken);
    }
}