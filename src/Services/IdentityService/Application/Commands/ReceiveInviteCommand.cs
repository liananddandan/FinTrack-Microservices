using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record ReceiveInviteCommand(string InvitationPublicId) : IRequest<ServiceResult<bool>>;