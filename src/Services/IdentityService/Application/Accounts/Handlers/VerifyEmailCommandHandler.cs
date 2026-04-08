using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

public class VerifyEmailCommandHandler(
    IEmailVerificationService emailVerificationService)
    : IRequestHandler<VerifyEmailCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        return await emailVerificationService.VerifyTokenAsync(
            request.Token,
            cancellationToken);
    }
}