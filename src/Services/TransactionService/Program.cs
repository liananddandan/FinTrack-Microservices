using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.Options;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Common.Options;
using TransactionService.Application.Middlewares;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Options;
using TransactionService.Application.Payments.Services;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Infrastructure.Audit;
using TransactionService.Infrastructure.Authentication;
using TransactionService.Infrastructure.Identity;
using TransactionService.Infrastructure.Payments;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<StripePaymentOptions>(
    builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<IdentityServiceOptions>(
    builder.Configuration.GetSection("IdentityService"));
builder.Services.Configure<StripeWebhookOptions>(
    builder.Configuration.GetSection("StripeWebhook"));

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
builder.Services.AddCap(options =>
{
    options.UseEntityFramework<TransactionDbContext>();
    options.UseRabbitMQ(cfg =>
    {
        cfg.HostName = builder.Configuration["CAP:RabbitMQ:HostName"]!;
        cfg.UserName = builder.Configuration["CAP:RabbitMQ:UserName"]!;
        cfg.Password = builder.Configuration["CAP:RabbitMQ:Password"]!;
    });
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IProductService).Assembly,
        typeof(ProductRepository).Assembly)
    .AddClasses(classes => classes.Where(type =>
        type.Name.EndsWith("Service") ||
        type.Name.EndsWith("Repository") ||
        type.Name.EndsWith("Repo")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();
builder.Services.AddScoped<IAuditLogPublisher, AuditLogPublisher>();
builder.Services.AddScoped<IPaymentProviderResolver, PaymentProviderResolver>();
builder.Services.AddScoped<IPaymentProvider, CashPaymentProvider>();
builder.Services.AddScoped<IPaymentProvider, StripeCardPaymentProvider>();
builder.Services.AddScoped<IStripePaymentGateway, StripePaymentGateway>();
builder.Services.AddHttpClient<ITenantPaymentConfigService, TenantPaymentConfigService>();
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