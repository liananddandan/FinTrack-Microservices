using System.Text;
using DotNetCore.CAP;
using IdentityService.Api.Common.Filters;
using IdentityService.Api.Common.Middlewares;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Options;
using IdentityService.Application.Common.Services;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Services;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Aduit.Publishers;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Repositories;
using IdentityService.Infrastructure.Platform;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<BootstrapAdminOptions>(
    builder.Configuration.GetSection("BootstrapAdmin"));
builder.Services.Configure<InternalApiOptions>(
    builder.Configuration.GetSection("InternalApi"));
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection("Frontend"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        var jwtTokenOptions = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>();
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtTokenOptions!.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.Secret)),
            ValidateAudience = true,
            ValidAudience = jwtTokenOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddCap(options =>
{
    options.UseEntityFramework<ApplicationIdentityDbContext>();
    var rabbitHost = builder.Configuration["CAP:RabbitMQ:HostName"];
    var rabbitUser = builder.Configuration["CAP:RabbitMQ:UserName"];
    var rabbitPassword = builder.Configuration["CAP:RabbitMQ:Password"];
    var rabbitVHost = builder.Configuration["CAP:RabbitMQ:VirtualHost"] ?? "/";

    Console.WriteLine("=== IDENTITY CAP CONFIG ===");
    Console.WriteLine($"HostName: {rabbitHost}");
    Console.WriteLine($"UserName: {rabbitUser}");
    Console.WriteLine($"Password empty: {string.IsNullOrWhiteSpace(rabbitPassword)}");
    Console.WriteLine($"VirtualHost: {rabbitVHost}");
    Console.WriteLine("===========================");
    if (string.IsNullOrWhiteSpace(rabbitHost) ||
        string.IsNullOrWhiteSpace(rabbitUser) ||
        string.IsNullOrWhiteSpace(rabbitPassword))
    {
        throw new InvalidOperationException("CAP RabbitMQ configuration is missing.");
    }
    options.UseRabbitMQ(cfg =>
    {
        cfg.HostName = builder.Configuration["CAP:RabbitMQ:HostName"]!;
        cfg.UserName = builder.Configuration["CAP:RabbitMQ:UserName"]!;
        cfg.Password = builder.Configuration["CAP:RabbitMQ:Password"]!;
    });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisCon = builder.Configuration.GetConnectionString("Redis")!;
    var configuration = ConfigurationOptions.Parse(redisCon, true);
    configuration.ResolveDns = true;
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>()
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();

// register MediatR
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalJwtTokenValidationFilter>();
    options.Filters.Add<InternalApiKeyValidationFilter>();
});

builder.Services.Scan(scan => scan
    .FromAssembliesOf(typeof(Program))
    .AddClasses(classes => classes.Where(type =>
        type.Name.EndsWith("Service") ||
        type.Name.EndsWith("Repo") ||
        type.Name.EndsWith("Repository")))
    .AsMatchingInterface()
    .WithScopedLifetime());

builder.Services.Scan(scan => scan
    .FromAssemblyOf<TenantDomainSubscriber>()
    .AddClasses(classes => classes.AssignableTo<ICapSubscribe>())
    .AsSelfWithInterfaces()
    .WithTransientLifetime());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuditLogPublisher, AuditLogPublisher>();
builder.Services.AddHostedService<BootstrapAdminHostedService>();
builder.Services.AddScoped<InternalApiKeyValidationFilter>();
builder.Services.AddScoped<ITenantContextResolver, TenantContextResolver>();
// builder.AddFinTrackOpenTelemetry();
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
            nameof(ApplicationIdentityDbContext),
            attempt,
            maxRetries);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation(
            "EF Core migrations applied for {DbContext}.",
            nameof(ApplicationIdentityDbContext));

        break;
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "Failed to apply EF Core migrations for {DbContext} on attempt {Attempt}/{MaxRetries}.",
            nameof(ApplicationIdentityDbContext),
            attempt,
            maxRetries);

        if (attempt == maxRetries)
        {
            app.Logger.LogCritical(
                ex,
                "Failed to apply EF Core migrations for {DbContext} after {MaxRetries} attempts. Service startup aborted.",
                nameof(ApplicationIdentityDbContext),
                maxRetries);

            throw;
        }

        await Task.Delay(delay);
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseFinTrackTelemetry();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<TenantContextResolutionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}