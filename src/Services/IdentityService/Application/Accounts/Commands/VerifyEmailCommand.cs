using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Commands;

public sealed record VerifyEmailCommand(string Token) : IRequest<ServiceResult<bool>>;