using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

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