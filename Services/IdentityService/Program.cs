using DotNetCore.CAP;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SharedKernel.Events;
using SharedKernel.Topics;

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

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var publisher = scope.ServiceProvider.GetRequiredService<ICapPublisher>();

    await publisher.PublishAsync(CapTopics.EmailSend, new EmailSendRequestedEvent
    {
        From = "test@test.com",
        To = "test@example.com",
        Subject = "RabbitMQ test",
        Body = "This is just a test."
    });
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
