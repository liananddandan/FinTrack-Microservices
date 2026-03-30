using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Services;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Tests.Fakes;

namespace TransactionService.Tests;

public class TransactionWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
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
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
            
            services.RemoveAll<IPaymentGateway>();
            services.AddScoped<IPaymentGateway, FakeStripePaymentGateway>();

            services.RemoveAll<IPaymentGatewayResolver>();
            services.AddScoped<IPaymentGatewayResolver, PaymentGatewayResolver>();

            lock (LockObject)
            {
                if (_databaseMigrated)
                {
                    return;
                }

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

                db.Database.EnsureDeleted();
                db.Database.Migrate();

                _databaseMigrated = true;
            }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Payments");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM OrderItems");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Orders");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Products");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ProductCategories");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM TenantAccounts");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Transactions");
    }
}