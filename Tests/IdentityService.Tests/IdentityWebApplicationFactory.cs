using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityService.Tests;

public class IdentityWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private static bool _dbInitialized = false;
    private static readonly object _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json");
            config.AddEnvironmentVariables();
        });
        builder.ConfigureServices(services =>
        {
            lock (_lock)
            {
                if (!_dbInitialized)
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.Migrate();

                    _dbInitialized = true;
                }
            }
        });
    }
}