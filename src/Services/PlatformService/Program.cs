using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlatformService.Application.Common.Abstractions;
using PlatformService.Application.Common.Options;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Handlers;
using PlatformService.Application.Tenants.Services;
using PlatformService.Infrastructure.ExternalServices;
using PlatformService.Infrastructure.Persistence;
using PlatformService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IdentityServiceOptions>(
    builder.Configuration.GetSection("IdentityService"));

builder.Services.AddHttpClient<IIdentityTenantDirectoryClient, IdentityTenantDirectoryClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<IdentityServiceOptions>>().Value;

        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    });

builder.Services.AddDbContext<PlatformDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
});

builder.Services.AddScoped<IPlatformTenantService, PlatformTenantService>();
builder.Services.AddScoped<ITenantDomainMappingRepository, TenantDomainMappingRepository>();
builder.Services.AddScoped<ITenantDomainMappingService, TenantDomainMappingService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddCap(options =>
{
    options.UseEntityFramework<PlatformDbContext>();

    options.UseRabbitMQ(rabbit =>
    {
        rabbit.HostName = builder.Configuration["CAP:RabbitMQ:HostName"];
        rabbit.UserName = builder.Configuration["CAP:RabbitMQ:UserName"];
        rabbit.Password = builder.Configuration["CAP:RabbitMQ:Password"];
    });

    options.DefaultGroupName = builder.Configuration["CAP:DefaultGroup"];
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetAllPlatformTenantsQueryHandler).Assembly);
});

var app = builder.Build();

// Apply database migrations before serving requests, with retry.
const int maxRetries = 10;
var delay = TimeSpan.FromSeconds(5);

for (var attempt = 1; attempt <= maxRetries; attempt++)
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();

        app.Logger.LogInformation(
            "Applying EF Core migrations for {DbContext}. Attempt {Attempt}/{MaxRetries}...",
            nameof(PlatformDbContext),
            attempt,
            maxRetries);

        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation(
            "EF Core migrations applied for {DbContext}.",
            nameof(PlatformDbContext));

        break;
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "Failed to apply EF Core migrations for {DbContext} on attempt {Attempt}/{MaxRetries}.",
            nameof(PlatformDbContext),
            attempt,
            maxRetries);

        if (attempt == maxRetries)
        {
            app.Logger.LogCritical(
                ex,
                "Failed to apply EF Core migrations for {DbContext} after {MaxRetries} attempts. Service startup aborted.",
                nameof(PlatformDbContext),
                maxRetries);

            throw;
        }

        await Task.Delay(delay);
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi("/openapi/v1.json");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}