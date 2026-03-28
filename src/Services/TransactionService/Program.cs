using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharedKernel.Common.Options;
using TransactionService.Application.Middlewares;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Repositories;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

builder.Services.AddScoped<ITransactionRepo, TransactionRepo>();
builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>() // 你也可以换成 typeof(Program) 或任何所在程序集的类型
    .AddClasses(classes => classes.InNamespaces(
        "TransactionService.Application.Services",
        "TransactionService.Infrastructure.Persistence.Repositories"))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddControllers();

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
            nameof(TransactionDbContext),
            attempt,
            maxRetries);

        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation(
            "EF Core migrations applied for {DbContext}.",
            nameof(TransactionDbContext));

        break;
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "Failed to apply EF Core migrations for {DbContext} on attempt {Attempt}/{MaxRetries}.",
            nameof(TransactionDbContext),
            attempt,
            maxRetries);

        if (attempt == maxRetries)
        {
            app.Logger.LogCritical(
                ex,
                "Failed to apply EF Core migrations for {DbContext} after {MaxRetries} attempts. Service startup aborted.",
                nameof(TransactionDbContext),
                maxRetries);

            throw;
        }

        await Task.Delay(delay);
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();


app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}