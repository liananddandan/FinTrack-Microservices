using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Account;

public record UserLoginCommand(string Email, string Password) : IRequest<ServiceResult<UserLoginResult>>;