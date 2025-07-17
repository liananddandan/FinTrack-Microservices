using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record ReceiveInviteCommand(string InvitationPublicId, string InvitationVersion) : IRequest<ServiceResult<bool>>;