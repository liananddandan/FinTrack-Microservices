using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Commands;


public sealed record ResendVerificationEmailCommand(
    string UserPublicId) : IRequest<ServiceResult<bool>>;