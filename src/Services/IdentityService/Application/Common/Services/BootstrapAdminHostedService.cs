using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Common.Services;
public class BootstrapAdminHostedService(
    IServiceProvider serviceProvider,
    ILogger<BootstrapAdminHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var bootstrapAdminService = scope.ServiceProvider.GetRequiredService<IBootstrapAdminService>();

            await bootstrapAdminService.EnsureBootstrapSuperAdminAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure bootstrap super admin.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}