using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.Options;
using IdentityService.Application.Platforms.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityService.Application.Common.Services;

public class BootstrapAdminService(
    UserManager<ApplicationUser> userManager,
    IPlatformAccessRepository platformAccessRepository,
    IUnitOfWork unitOfWork,
    IOptions<BootstrapAdminOptions> bootstrapOptions,
    ILogger<BootstrapAdminService> logger)
    : IBootstrapAdminService
{
    private readonly BootstrapAdminOptions _options = bootstrapOptions.Value;

    public async Task EnsureBootstrapSuperAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Bootstrap admin is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Email) ||
            string.IsNullOrWhiteSpace(_options.UserName) ||
            string.IsNullOrWhiteSpace(_options.Password))
        {
            logger.LogWarning("Bootstrap admin configuration is incomplete. Skipping bootstrap.");
            return;
        }

        var user = await userManager.FindByEmailAsync(_options.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = _options.UserName,
                Email = _options.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, _options.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
                logger.LogError("Failed to create bootstrap admin user: {Errors}", errors);
                return;
            }

            logger.LogInformation(
                "Bootstrap admin user created successfully. UserPublicId: {UserPublicId}",
                user.PublicId);
        }
        else
        {
            logger.LogInformation(
                "Bootstrap admin user already exists. UserPublicId: {UserPublicId}",
                user.PublicId);
        }

        var existingPlatformAccess = await platformAccessRepository.GetByUserPublicIdAsync(
            user.PublicId.ToString(),
            cancellationToken);

        if (existingPlatformAccess is not null)
        {
            if (!existingPlatformAccess.IsEnabled || existingPlatformAccess.Role != _options.PlatformRole)
            {
                existingPlatformAccess.IsEnabled = true;
                existingPlatformAccess.Role = _options.PlatformRole;
                existingPlatformAccess.UpdatedAt = DateTime.UtcNow;

                await platformAccessRepository.UpdateAsync(existingPlatformAccess, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Bootstrap platform access updated for user {UserPublicId}. Role: {Role}",
                    user.PublicId,
                    _options.PlatformRole);
            }
            else
            {
                logger.LogInformation(
                    "Bootstrap platform access already exists for user {UserPublicId}.",
                    user.PublicId);
            }

            return;
        }

        var platformAccess = new PlatformAccess
        {
            UserPublicId = user.PublicId.ToString(),
            Role = _options.PlatformRole,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        await platformAccessRepository.AddAsync(platformAccess, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Bootstrap platform access created for user {UserPublicId}. Role: {Role}",
            user.PublicId,
            _options.PlatformRole);
    }
}