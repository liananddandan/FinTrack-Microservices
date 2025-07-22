using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record ConfirmAccountEmailCommand(string UserPublicId, string Token) : IRequest<ServiceResult<ConfirmAccountEmailResult>>;