using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record SetUserPasswordCommand(
    string UserPublicId, 
    string OldPassword, 
    string NewPassword,
    bool Reset) : IRequest<ServiceResult<bool>>;