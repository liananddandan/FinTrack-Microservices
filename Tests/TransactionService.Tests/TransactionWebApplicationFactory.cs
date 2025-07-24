using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.ExternalServices;
using TransactionService.ExternalServices.Interfaces;
using TransactionService.Infrastructure;
using TransactionService.Services.Interfaces;

namespace TransactionService.Tests;

public class TransactionWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private static bool _dbInitialized = false;
    private static readonly object _lock = new();
    private readonly string _mockBaseUrl;

    public TransactionWebApplicationFactory(string mockBaseUrl)
    {
        _mockBaseUrl = mockBaseUrl;
    }

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
                    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
                    db.Database.Migrate();
                    db.Database.ExecuteSqlRaw("DELETE FROM Transactions");
                    _dbInitialized = true;
                }
            }
            
            var descriptors = services.Where(d => d.ServiceType == typeof(IIdentityClientService)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddHttpClient<IIdentityClientService, IdentityClientService>(client =>
            {
                client.BaseAddress = new Uri(_mockBaseUrl);
            });
        });
    }
}