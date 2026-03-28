using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class ConfirmAccountEmailCommandHandler(IUserAppService userAppService)
    : IRequestHandler<ConfirmAccountEmailCommand, ServiceResult<ConfirmAccountEmailResult>>
{
    public Task<ServiceResult<ConfirmAccountEmailResult>> Handle(ConfirmAccountEmailCommand request, CancellationToken cancellationToken)
    {
        return userAppService.ConfirmAccountEmailAsync(request.UserPublicId, request.Token, cancellationToken);
    }
}