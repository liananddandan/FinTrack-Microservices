using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class ConfirmAccountEmailCommandHandler(IUserAppService userAppService)
    : IRequestHandler<ConfirmAccountEmailCommand, ServiceResult<ConfirmAccountEmailResult>>
{
    public Task<ServiceResult<ConfirmAccountEmailResult>> Handle(ConfirmAccountEmailCommand request, CancellationToken cancellationToken)
    {
        return userAppService.ConfirmAccountEmailAsync(request.UserPublicId, request.Token, cancellationToken);
    }
}