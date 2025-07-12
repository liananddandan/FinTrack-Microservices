using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class SetUserPasswordCommandHandler(
    IUserAppService userAppService) : IRequestHandler<SetUserPasswordCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        return userAppService.SetUserPasswordAsync(request.UserPublicId, request.JwtVersion, request.OldPassword,
            request.NewPassword, request.Reset, cancellationToken);
    }
}