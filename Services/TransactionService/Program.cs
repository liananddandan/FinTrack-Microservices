using System.Text;
using IdentityService.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharedKernel.Common.Options;
using TransactionService.ExternalServices;
using TransactionService.ExternalServices.Interfaces;
using TransactionService.Filters;
using TransactionService.Infrastructure;
using TransactionService.Services;
using TransactionService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddMediatR(configuration => 
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>() // 你也可以换成 typeof(Program) 或任何所在程序集的类型
    .AddClasses(classes => classes.InNamespaces(
        "TransactionService.Services",
        "TransactionService.Repositories"))
    .AsMatchingInterface() // 如 UserService -> IUserService
    .WithScopedLifetime());

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add(new RequireClaimsFilter());
});

builder.Services.AddHttpClient<IIdentityClientService, IdentityClientService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetSection("IdentityService").GetValue<string>("BaseUrl")!);
    }
);

var app = builder.Build();

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
public partial class Program { }
