using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class ChangeUserPasswordCommandHandler(
    IUserAppService userAppService) : IRequestHandler<ChangeUserPasswordCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        return userAppService.SetUserPasswordAsync(request.UserPublicId, request.JwtVersion, request.OldPassword,
            request.NewPassword, cancellationToken);
    }
}