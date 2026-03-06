using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record ConfirmAccountEmailCommand(string UserPublicId, string Token) : IRequest<ServiceResult<ConfirmAccountEmailResult>>;