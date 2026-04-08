using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Accounts.Events;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Handlers;

public class ResendVerificationEmailCommandHandler(
    IApplicationUserRepo applicationUserRepo,
    IEmailVerificationService emailVerificationService,
    IEmailThrottleService emailThrottleService,
    IMediator mediator)
    : IRequestHandler<ResendVerificationEmailCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        ResendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await applicationUserRepo.GetUserByPublicIdAsync(
            request.UserPublicId,
            cancellationToken);

        if (user is null)
        {
            return ServiceResult<bool>.Fail(
                "EMAIL_VERIFICATION_USER_NOT_FOUND",
                "User not found.");
        }

        if (user.EmailConfirmed)
        {
            return ServiceResult<bool>.Fail(
                "EMAIL_VERIFICATION_ALREADY_CONFIRMED",
                "Email is already verified.");
        }

        var throttleResult = await emailThrottleService.CheckVerificationResendAllowedAsync(
            user.Id,
            user.Email!,
            cancellationToken);
        if (!throttleResult.Success)
        {
            return ServiceResult<bool>.Fail(
                throttleResult.Code!,
                throttleResult.Message!);
        }

        var verificationResult = await emailVerificationService.CreateTokenAsync(
            user.Id,
            createdByIp: null,
            cancellationToken: cancellationToken);

        if (!verificationResult.Success || verificationResult.Data is null)
        {
            return ServiceResult<bool>.Fail(
                verificationResult.Code ?? "EMAIL_VERIFICATION_TOKEN_CREATE_FAILED",
                verificationResult.Message ?? "Failed to create email verification token.");
        }

        await mediator.Publish(
            new SendEmailVerificationRequestedEvent(
                user.Id,
                user.Email!,
                user.UserName!,
                verificationResult.Data.RawToken,
                verificationResult.Data.ExpiresAtUtc),
            cancellationToken);
        await emailThrottleService.MarkVerificationResendSentAsync(
            user.Id,
            user.Email!,
            cancellationToken);
        return ServiceResult<bool>.Ok(
            true,
            "EMAIL_VERIFICATION_RESENT",
            "Verification email sent successfully.");
    }
}