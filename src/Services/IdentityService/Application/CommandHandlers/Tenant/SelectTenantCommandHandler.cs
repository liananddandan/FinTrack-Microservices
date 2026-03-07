using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class SelectTenantCommandHandler(IAccountService accountService)
    : IRequestHandler<SelectTenantCommand, ServiceResult<string>>
{
    public async Task<ServiceResult<string>> Handle(
        SelectTenantCommand request,
        CancellationToken cancellationToken)
    {
        return await accountService.SelectTenantAsync(
            request.UserPublicId,
            request.TenantPublicId,
            cancellationToken);
    }
}