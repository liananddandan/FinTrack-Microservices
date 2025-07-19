using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class RefreshUserJwtTokenCommandHandler(IUserAppService userAppService)
    : IRequestHandler<RefreshUserJwtTokenCommand, ServiceResult<JwtTokenPair>>
{
    public async Task<ServiceResult<JwtTokenPair>> Handle(RefreshUserJwtTokenCommand request, CancellationToken cancellationToken)
    {
        return await userAppService.RefreshUserTokenPairAsync(request.UserPublicId, cancellationToken);
    }
}