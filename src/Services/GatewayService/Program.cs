using System.Text;
using GatewayService.Application.Common.Middlewares;
using GatewayService.Application.Common.Options;
using GatewayService.Application.Dev.Abstractions;
using GatewayService.Application.Dev.Services;
using GatewayService.Application.Doc.Abstractions;
using GatewayService.Application.Doc.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharedKernel.Common.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetSection("Authentication"));

// add yarp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<OpenApiServicesOptions>(
    builder.Configuration.GetSection("OpenApiServices"));

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

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisCon = builder.Configuration.GetConnectionString("Redis")!;
    var configuration = ConfigurationOptions.Parse(redisCon, true);
    configuration.ResolveDns = true;
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5176",
                "http://localhost:5175",
                "http://localhost:5177",
                "http://fintrack.chenlis.local:5175",
                "http://coffee.chenlis.local:5175",
                "http://sushi.chenlis.local:5175",
                "http://fintrack.chenlis.local:5176",
                "http://coffee.chenlis.local:5176",
                "http://sushi.chenlis.local:5176")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IDevSeedOrchestrator, DevSeedOrchestrator>();
builder.Services.AddSingleton<IOpenApiDocumentMerger, OpenApiDocumentMerger>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<BasicJwtTokenValidationMiddleware>();
app.MapControllers();
app.MapGet("/api/openapi/all.json", async (
    IHttpClientFactory httpClientFactory,
    IOptions<OpenApiServicesOptions> options,
    IOpenApiDocumentMerger merger) =>
{
    var client = httpClientFactory.CreateClient();
    var config = options.Value;
    var identityUrl = $"{config.Identity}/openapi/v1.json";
    var transactionUrl = $"{config.Transaction}/openapi/v1.json";
    var auditLogUrl = $"{config.AuditLog}/openapi/v1.json";

    Console.WriteLine($"[OpenAPI] Identity URL: {identityUrl}");
    Console.WriteLine($"[OpenAPI] Transaction URL: {transactionUrl}");
    Console.WriteLine($"[OpenAPI] AuditLog URL: {auditLogUrl}");

    var identityJson = await client.GetStringAsync(identityUrl);
    var transactionJson = await client.GetStringAsync(transactionUrl);
    var auditLogJson = await client.GetStringAsync(auditLogUrl);
    var merged = merger.Merge(
        ("Identity API", identityJson),
        ("Transaction API", transactionJson),
        ("AuditLog API", auditLogJson));

    return Results.Content(merged, "application/json");
});

app.MapScalarApiReference("/api/swagger", options =>
{
    options.WithTitle("Transaction & Workflow Platform API Docs");
    options.WithTheme(ScalarTheme.Default);
    options.OpenApiRoutePattern = "/api/openapi/all.json";
});

app.MapReverseProxy();
app.Run();