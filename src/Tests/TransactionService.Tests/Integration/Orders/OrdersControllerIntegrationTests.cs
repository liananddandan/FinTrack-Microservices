using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Orders.Contracts;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;
using Xunit;

namespace TransactionService.Tests.Integration.Orders;

[Collection("NonParallel Collection")]
public class OrdersControllerIntegrationTests
    : IClassFixture<TransactionWebApplicationFactory<Program>>,
      IAsyncLifetime
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private readonly Guid _tenantPublicId = Guid.NewGuid();
    private readonly Guid _userPublicId = Guid.NewGuid();

    public OrdersControllerIntegrationTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        _client.SetTestAuth(
            role: "Admin",
            userPublicId: _userPublicId,
            tenantPublicId: _tenantPublicId);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_ShouldReturn200_WhenRequestIsValid()
    {
        var categoryPublicId = await SeedCategoryAsync("Coffee");
        var productPublicId = await SeedProductAsync(categoryPublicId, "Latte", 5.50m);

        var request = new CreateOrderRequest(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemRequest>
            {
                new(productPublicId, 2, "Less sugar")
            });

        var response = await _client.PostAsJsonAsync("/api/orders", request);
        var raw = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, raw);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.CreateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.CustomerName.Should().Be("Emily");
        body.Data.PaymentMethod.Should().Be(PaymentMethods.Cash);
        body.Data.Items.Should().HaveCount(1);
        body.Data.Items[0].ProductNameSnapshot.Should().Be("Latte");
        body.Data.Items[0].Quantity.Should().Be(2);
        body.Data.SubtotalAmount.Should().Be(9.57m);
        body.Data.GstAmount.Should().Be(1.43m);
        body.Data.TotalAmount.Should().Be(11.00m);

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var savedOrder = await db.Orders
            .Include(x => x.Items)
            .SingleAsync();

        savedOrder.TenantPublicId.Should().Be(_tenantPublicId);
        savedOrder.CustomerName.Should().Be("Emily");
        savedOrder.CreatedByUserPublicId.Should().Be(_userPublicId);
        savedOrder.Items.Should().HaveCount(1);
        savedOrder.Items.First().ProductNameSnapshot.Should().Be("Latte");
        savedOrder.Items.First().LineTotal.Should().Be(11.00m);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenProductNotFound()
    {
        var request = new CreateOrderRequest(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemRequest>
            {
                new(Guid.NewGuid(), 1, null)
            });

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.ProductNotFound);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenOrderExists()
    {
        var orderPublicId = await SeedOrderAsync();

        var response = await _client.GetAsync($"/api/orders/{orderPublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.GetByIdSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.PublicId.Should().Be(orderPublicId);
        body.Data.OrderNumber.Should().NotBeNullOrWhiteSpace();
        body.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ShouldReturn400_WhenOrderNotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.NotFound);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOrdersForTenant()
    {
        await SeedOrderAsync(customerName: "Emily");
        await SeedOrderAsync(customerName: "Jack");

        var response = await _client.GetAsync("/api/orders?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<OrderListItemDto>>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.GetPagedSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Items.Should().HaveCount(2);
        body.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOnlyCurrentUserOrders_WhenCreatedByMeIsTrue()
    {
        await SeedOrderAsync(customerName: "Mine", createdByUserPublicId: _userPublicId);
        await SeedOrderAsync(customerName: "Other", createdByUserPublicId: Guid.NewGuid());

        var response = await _client.GetAsync("/api/orders?createdByMe=true&pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<OrderListItemDto>>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.GetPagedSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Items.Should().HaveCount(1);
        body.Data.Items[0].CustomerName.Should().Be("Mine");
    }

    [Fact]
    public async Task Cancel_ShouldReturn200_WhenOrderExists()
    {
        var orderPublicId = await SeedOrderAsync();

        var response = await _client.PostAsync($"/api/orders/{orderPublicId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.CancelSuccess);
        body.Data.Should().BeTrue();

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var order = await db.Orders.SingleAsync(x => x.PublicId == orderPublicId);
        order.Status.Should().Be(OrderStatuses.Cancelled);
        order.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Cancel_ShouldReturn400_WhenOrderAlreadyCancelled()
    {
        var orderPublicId = await SeedOrderAsync(status: OrderStatuses.Cancelled);

        var response = await _client.PostAsync($"/api/orders/{orderPublicId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.AlreadyCancelled);
        body.Data.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetSummary_ShouldReturnTenantSummary()
    {
        await SeedOrderAsync(customerName: "Emily", totalAmount: 11.50m, status: OrderStatuses.Completed);
        await SeedOrderAsync(customerName: "Jack", totalAmount: 23.00m, status: OrderStatuses.Completed);
        await SeedOrderAsync(customerName: "Cancelled", totalAmount: 9.20m, status: OrderStatuses.Cancelled);

        var response = await _client.GetAsync("/api/orders/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderSummaryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.GetSummarySuccess);
        body.Data.Should().NotBeNull();
        body.Data!.OrderCount.Should().Be(2);
        body.Data.TotalRevenue.Should().Be(34.50m);
        body.Data.AverageOrderValue.Should().Be(17.25m);
        body.Data.CancelledOrderCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOnlyCurrentUserSummary_WhenCreatedByMeIsTrue()
    {
        await SeedOrderAsync(customerName: "Mine 1", createdByUserPublicId: _userPublicId, totalAmount: 10m, status: OrderStatuses.Completed);
        await SeedOrderAsync(customerName: "Mine 2", createdByUserPublicId: _userPublicId, totalAmount: 20m, status: OrderStatuses.Completed);
        await SeedOrderAsync(customerName: "Other", createdByUserPublicId: Guid.NewGuid(), totalAmount: 99m, status: OrderStatuses.Completed);

        var response = await _client.GetAsync("/api/orders/summary?createdByMe=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<OrderSummaryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Order.GetSummarySuccess);
        body.Data.Should().NotBeNull();
        body.Data!.OrderCount.Should().Be(2);
        body.Data.TotalRevenue.Should().Be(30m);
        body.Data.AverageOrderValue.Should().Be(15m);
    }

    private async Task<Guid> SeedCategoryAsync(string name)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var category = new ProductCategory
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            Name = name,
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.ProductCategories.Add(category);
        await db.SaveChangesAsync();

        return category.PublicId;
    }

    private async Task<Guid> SeedProductAsync(Guid categoryPublicId, string name, decimal price)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var category = await db.ProductCategories.SingleAsync(x => x.PublicId == categoryPublicId);

        var product = new Product
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            CategoryId = category.Id,
            Name = name,
            Price = price,
            DisplayOrder = 1,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.PublicId;
    }

    private async Task<Guid> SeedOrderAsync(
        string customerName = "Emily",
        Guid? createdByUserPublicId = null,
        string status = OrderStatuses.Completed,
        decimal totalAmount = 11.50m)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var subtotal = Math.Round(totalAmount / 1.15m, 2, MidpointRounding.AwayFromZero);
        var gstAmount = totalAmount - subtotal;

        var order = new Order
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21],
            CustomerName = customerName,
            CustomerPhone = "0211234567",
            CreatedByUserPublicId = createdByUserPublicId ?? _userPublicId,
            CreatedByUserNameSnapshot = "Test User",
            SubtotalAmount = subtotal,
            GstRate = 0.15m,
            GstAmount = gstAmount,
            DiscountAmount = 0m,
            TotalAmount = totalAmount,
            Status = status,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = PaymentMethods.Cash,
            CreatedAt = DateTime.UtcNow,
            PaidAt = status == OrderStatuses.Cancelled ? null : DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductPublicId = Guid.NewGuid(),
                    ProductNameSnapshot = "Latte",
                    UnitPrice = subtotal,
                    Quantity = 1,
                    LineTotal = subtotal,
                    Notes = null
                }
            }
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.PublicId;
    }

    private AsyncServiceScope CreateScope()
    {
        return _factory.Services.CreateAsyncScope();
    }
}