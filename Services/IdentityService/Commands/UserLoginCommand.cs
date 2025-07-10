using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public class UserLoginCommand : IRequest<ServiceResult<UserLoginResult>>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}