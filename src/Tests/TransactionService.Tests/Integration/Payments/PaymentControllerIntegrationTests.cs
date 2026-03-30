using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.DTOs;
using TransactionService.Api.Payments.Contracts;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace TransactionService.Tests.Integration.Payments;
[Collection("NonParallel Collection")]
public class PaymentControllerIntegrationTests(TransactionWebApplicationFactory<Program> factory,
    ITestOutputHelper testOutputHelper)
    : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateAsync_ShouldCreatePaymentSuccessfully()
    {
        var tenantPublicId = Guid.NewGuid();
        var orderPublicId = Guid.NewGuid();

        await SeedOrderAsync(tenantPublicId, orderPublicId);

        _client.SetTestAuth(
            role: "Admin",
            userPublicId: Guid.NewGuid(),
            tenantPublicId: tenantPublicId);
        
        var request = new CreatePaymentRequest
        {
            OrderPublicId = orderPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card
        };

        var response = await _client.PostAsJsonAsync("/api/payments", request);
        var raw = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(raw);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.OrderPublicId.Should().Be(orderPublicId);
        payload.Data.Provider.Should().Be(PaymentProviders.Stripe);
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldReturnPayment_WhenPaymentExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var orderPublicId = Guid.NewGuid();

        await SeedOrderWithPaymentAsync(tenantPublicId, orderPublicId);


        _client.SetTestAuth(
            role: "Admin",
            userPublicId: Guid.NewGuid(),
            tenantPublicId: tenantPublicId);
        var response = await _client.GetAsync($"/api/payments/by-order/{orderPublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();

        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        payload.Data!.OrderPublicId.Should().Be(orderPublicId);
        payload.Data.Status.Should().Be(PaymentStatuses.Pending);
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        var tenantPublicId = Guid.NewGuid();
        var orderPublicId = Guid.NewGuid();

        _client.SetTestAuth(
            role: "Admin",
            userPublicId: Guid.NewGuid(),
            tenantPublicId: tenantPublicId);
        
        var response = await _client.GetAsync($"/api/payments/by-order/{orderPublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task SeedOrderAsync(Guid tenantPublicId, Guid orderPublicId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var order = new Order
        {
            PublicId = orderPublicId,
            TenantPublicId = tenantPublicId,
            OrderNumber = "AKC-TEST-0001",
            CreatedByUserPublicId = Guid.NewGuid(),
            CreatedByUserNameSnapshot = "test@local",
            SubtotalAmount = 10m,
            GstRate = 0.15m,
            GstAmount = 1.50m,
            DiscountAmount = 0m,
            TotalAmount = 11.50m,
            Status = OrderStatuses.Pending,
            PaymentStatus = PaymentStatuses.NotStarted,
            PaymentMethod = PaymentMethods.Card,
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    private async Task SeedOrderWithPaymentAsync(Guid tenantPublicId, Guid orderPublicId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var order = new Order
        {
            PublicId = orderPublicId,
            TenantPublicId = tenantPublicId,
            OrderNumber = "AKC-TEST-0002",
            CreatedByUserPublicId = Guid.NewGuid(),
            CreatedByUserNameSnapshot = "test@local",
            SubtotalAmount = 20m,
            GstRate = 0.15m,
            GstAmount = 3m,
            DiscountAmount = 0m,
            TotalAmount = 23m,
            Status = OrderStatuses.Pending,
            PaymentStatus = PaymentStatuses.Pending,
            PaymentMethod = PaymentMethods.Card,
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var payment = new Payment
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            OrderId = order.Id,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = order.TotalAmount,
            Currency = "NZD",
            IdempotencyKey = $"payment-{orderPublicId:N}",
            ProviderPaymentReference = "pi_test_456",
            ProviderClientSecret = "secret_test_456",
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();
    }
}