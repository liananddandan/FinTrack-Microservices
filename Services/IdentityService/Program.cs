using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Middlewares;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();

// register MediatR
builder.Services.AddMediatR(configuration => 
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>() // 你也可以换成 typeof(Program) 或任何所在程序集的类型
    .AddClasses(classes => classes.InNamespaces(
        "IdentityService.Services",
        "IdentityService.Repositories"))
    .AsMatchingInterface() // 如 UserService -> IUserService
    .WithScopedLifetime());
builder.Services.AddScoped<IUserDomainService, UserService>();
builder.Services.AddScoped<IUserAppService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }