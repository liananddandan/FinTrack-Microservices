using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

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