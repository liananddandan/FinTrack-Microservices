using IdentityService.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record UserLoginCommand(string Email, string Password) : IRequest<ServiceResult<UserLoginResult>>;