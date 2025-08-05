using System.Text;
using GatewayService.Common.Options;
using GatewayService.Middlewares;
using Microsoft.IdentityModel.Tokens;
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

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BasicJwtTokenValidationMiddleware>();
app.MapReverseProxy();
app.Run();
