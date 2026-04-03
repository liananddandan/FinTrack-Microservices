using IdentityService.Application.Platforms.Abstractions;
using IdentityService.Application.Platforms.Commands;
using IdentityService.Application.Platforms.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Platforms.Handlers;

public class SelectPlatformCommandHandler(
    IPlatformAccessService platformAccessService)
    : IRequestHandler<SelectPlatformCommand, ServiceResult<PlatformTokenDto>>
{
    public async Task<ServiceResult<PlatformTokenDto>> Handle(
        SelectPlatformCommand request,
        CancellationToken cancellationToken)
    {
        return await platformAccessService.SelectPlatformAsync(
            request.UserPublicId,
            cancellationToken);
    }
}