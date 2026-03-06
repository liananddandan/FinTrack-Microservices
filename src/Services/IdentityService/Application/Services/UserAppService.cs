using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Application.Services;

public class UserAppService(UserManager<ApplicationUser> userManager, 
    IUserVerificationService userVerificationService,
    IUserDomainService userDomainService,
    IMediator mediator,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    IConnectionMultiplexer redis) : IUserAppService
{
    public Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ConfirmAccountEmailResult>> ConfirmAccountEmailAsync(string userPublicId, string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserLoginResult>> UserLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> SetUserPasswordAsync(string userPublicId, string oldPassword, string newPassword, bool reset,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<JwtTokenPair>> RefreshUserTokenPairAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserInfoDto>> GetUserInfoAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}