using IdentityService.Application.Platforms.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Platforms.Commands;

public record SelectPlatformCommand(
    string UserPublicId) : IRequest<ServiceResult<PlatformTokenDto>>;