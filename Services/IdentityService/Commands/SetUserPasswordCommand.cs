using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record SetUserPasswordCommand(
    string UserPublicId, 
    string JwtVersion, 
    string OldPassword, 
    string NewPassword,
    bool Reset) : IRequest<ServiceResult<bool>>;