using System.Text;
using IdentityService.Api.Filters;
using IdentityService.Api.Middlewares;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharedKernel.Common.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));

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
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});

builder.Services.AddCap(options =>
{
    options.UseEntityFramework<ApplicationIdentityDbContext>();
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
builder.Services.AddControllers(options => { options.Filters.Add<GlobalJwtTokenValidationFilter>(); });

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.InNamespaces(
        "IdentityService.Application.Services",
        "IdentityService.Infrastructure.Persistence.Repositories"))
    .AsMatchingInterface()
    .WithScopedLifetime());

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}