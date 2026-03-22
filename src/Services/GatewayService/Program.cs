using System.Text;
using GatewayService.Application.Common.Options;
using GatewayService.Application.Middlewares;
using GatewayService.Application.Services;
using GatewayService.Application.Services.Interfaces;
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
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
            .WithOrigins("http://localhost:5173", 
                "http://localhost:5174", 
                "http://localhost:5175", 
                "http://localhost:5176")
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
app.MapGet("/api/openapi/all.json", async (IHttpClientFactory httpClientFactory,
    IOpenApiDocumentMerger merger) =>
{
    var client = httpClientFactory.CreateClient();

    var identityJson = await client.GetStringAsync("http://localhost:5100/openapi/v1.json");
    var transactionJson = await client.GetStringAsync("http://localhost:5133/openapi/v1.json");
    var auditLogJson = await client.GetStringAsync("http://localhost:5107/openapi/v1.json");
    
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
