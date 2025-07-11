using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record ChangeUserPasswordCommand(
    string UserPublicId, 
    string JwtVersion, 
    string OldPassword, 
    string NewPassword) : IRequest<ServiceResult<bool>>;