using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Handlers;
using NotificationService.Application.Options;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<CapDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddCap(options =>
{
    options.UseEntityFramework<CapDbContext>();
    options.UseRabbitMQ(cfg =>
    {
        cfg.HostName = builder.Configuration["CAP:RabbitMQ:HostName"]!;
        cfg.UserName = builder.Configuration["CAP:RabbitMQ:UserName"]!;
        cfg.Password = builder.Configuration["CAP:RabbitMQ:Password"]!;
    });
});

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<ResendOptions>(
    builder.Configuration.GetSection("Resend"));

builder.Services.Scan(scan => scan
    .FromAssemblyOf<EmailSendEventHandler>()
    .AddClasses(classes => classes.AssignableTo<ICapSubscribe>())
    .AsSelfWithInterfaces()
    .WithTransientLifetime());

var emailProvider = builder.Configuration.GetValue<string>("Email:Provider");

if (string.Equals(emailProvider, "Resend", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IEmailService, ResendEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();