using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using NotificationService.Handlers;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Options;
using NotificationService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(options => options.AddConsole());

builder.Services.AddDbContext<CapDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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

builder.Services.Scan(scan => scan
    .FromAssemblyOf<EmailSendEventHandler>()
    .AddClasses(classes => classes.AssignableTo<ICapSubscribe>())
    .AsSelfWithInterfaces()
    .WithTransientLifetime());

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapControllers();

app.Run();