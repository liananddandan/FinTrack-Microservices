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
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetAllPlatformTenantsQueryHandler).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}