using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Results;
using TransactionService.Api.Products.Contracts;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;
using Xunit;

namespace TransactionService.Tests.Integration.Products;

[Collection("NonParallel Collection")]
public class ProductsControllerIntegrationTests
    : IClassFixture<TransactionWebApplicationFactory<Program>>,
      IAsyncLifetime
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private readonly Guid _tenantPublicId = Guid.NewGuid();
    private readonly Guid _userPublicId = Guid.NewGuid();

    public ProductsControllerIntegrationTests(TransactionWebApplicationFactory<Program> factory)
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

        var request = new CreateProductRequest(
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.CreateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Flat White");
        body.Data.CategoryPublicId.Should().Be(categoryPublicId);

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var saved = await db.Products.SingleAsync(x => x.Name == "Flat White");
        saved.TenantPublicId.Should().Be(_tenantPublicId);
        saved.Price.Should().Be(5.5m);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenCategoryNotFound()
    {
        var request = new CreateProductRequest(
            Guid.NewGuid(),
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.CategoryNotFound);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameAlreadyExistsInSameCategory()
    {
        var categoryPublicId = await SeedCategoryAsync("Coffee");
        await SeedProductAsync(categoryPublicId, "Flat White", 5.5m);

        var request = new CreateProductRequest(
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.8m,
            null,
            2);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.CreateDuplicatedName);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WhenRequestIsValid()
    {
        var categoryPublicId = await SeedCategoryAsync("Coffee");
        var productPublicId = await SeedProductAsync(categoryPublicId, "Flat White", 5.5m);

        var request = new UpdateProductRequest(
            categoryPublicId,
            "Latte",
            "Coffee",
            5.8m,
            "https://example.com/latte.png",
            3,
            false);

        var response = await _client.PutAsJsonAsync($"/api/products/{productPublicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.UpdateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Latte");
        body.Data.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ShouldReturn200_WhenProductExists()
    {
        var categoryPublicId = await SeedCategoryAsync("Coffee");
        var productPublicId = await SeedProductAsync(categoryPublicId, "Flat White", 5.5m);

        var response = await _client.DeleteAsync($"/api/products/{productPublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.DeleteSuccess);
        body.Data.Should().BeTrue();

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var deleted = await db.Products
            .IgnoreQueryFilters()
            .SingleAsync(x => x.PublicId == productPublicId);

        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategory_ShouldReturnProductsUnderCategory()
    {
        var categoryPublicId = await SeedCategoryAsync("Coffee");
        await SeedProductAsync(categoryPublicId, "Flat White", 5.5m);
        await SeedProductAsync(categoryPublicId, "Latte", 5.8m);

        var response = await _client.GetAsync($"/api/product-categories/{categoryPublicId}/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.Product.GetListSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Should().HaveCount(2);
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

    private AsyncServiceScope CreateScope()
    {
        return _factory.Services.CreateAsyncScope();
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class ProductDto
    {
        public Guid PublicId { get; set; }
        public Guid CategoryPublicId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsAvailable { get; set; }
    }
}