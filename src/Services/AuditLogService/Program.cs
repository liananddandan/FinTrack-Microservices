using System.Text;
using AuditLogService.Application.Interfaces;
using AuditLogService.Application.Services;
using AuditLogService.Infrastructure.CapSubscribers;
using AuditLogService.Infrastructure.Persistence;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<AuditLogSubscriber>()
    .AddClasses(classes => classes.AssignableTo<ICapSubscribe>())
    .AsSelfWithInterfaces()
    .WithTransientLifetime());

builder.Services.AddDbContext<AuditLogDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddCap(x =>
{
    x.UseEntityFramework<AuditLogDbContext>();

    x.UseRabbitMQ(options =>
    {
        options.HostName = builder.Configuration["CAP:RabbitMQ:HostName"]
                           ?? throw new InvalidOperationException("CAP RabbitMQ HostName is not configured.");

        options.UserName = builder.Configuration["CAP:RabbitMQ:UserName"]
                           ?? throw new InvalidOperationException("CAP RabbitMQ UserName is not configured.");

        options.Password = builder.Configuration["CAP:RabbitMQ:Password"]
                           ?? throw new InvalidOperationException("CAP RabbitMQ Password is not configured.");
    });
    x.DefaultGroupName = builder.Configuration["CAP:DefaultGroup"] ?? "audit-log.service";
    x.FailedRetryCount = 3;
});

builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IAuditLogReader, AuditLogReader>();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});
builder.Services.AddAuthorization();

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
            nameof(AuditLogDbContext),
            attempt,
            maxRetries);

        var dbContext = scope.ServiceProvider.GetRequiredService<AuditLogDbContext>();
        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation(
            "EF Core migrations applied for {DbContext}.",
            nameof(AuditLogDbContext));

        break;
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "Failed to apply EF Core migrations for {DbContext} on attempt {Attempt}/{MaxRetries}.",
            nameof(AuditLogDbContext),
            attempt,
            maxRetries);

        if (attempt == maxRetries)
        {
            app.Logger.LogCritical(
                ex,
                "Failed to apply EF Core migrations for {DbContext} after {MaxRetries} attempts. Service startup aborted.",
                nameof(AuditLogDbContext),
                maxRetries);

            throw;
        }

        await Task.Delay(delay);
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();