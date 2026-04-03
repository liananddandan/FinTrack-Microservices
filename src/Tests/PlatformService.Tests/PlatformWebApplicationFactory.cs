using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Infrastructure.Persistence;

namespace PlatformService.Tests;

public class PlatformWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private static readonly object LockObject = new();
    private static bool _databaseMigrated;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            lock (LockObject)
            {
                if (_databaseMigrated)
                {
                    return;
                }

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

                db.Database.Migrate();

                _databaseMigrated = true;
            }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        await db.Database.ExecuteSqlRawAsync("DELETE FROM TenantDomainMappings");
    }
}