using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class SetUserPasswordCommandHandler(
    IUserAppService userAppService) : IRequestHandler<SetUserPasswordCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        return userAppService.SetUserPasswordAsync(request.UserPublicId, request.OldPassword,
            request.NewPassword, request.Reset, cancellationToken);
    }
}