using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Results;
using TransactionService.Api.ProductCategories.Contracts;
using TransactionService.Application.ProductCategories.Dtos;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Tests.Integration.ProductCategories;

[Collection("NonParallel Collection")]
public class ProductCategoriesControllerIntegrationTests
    : IClassFixture<TransactionWebApplicationFactory<Program>>,
      IAsyncLifetime
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private readonly Guid _tenantPublicId = Guid.NewGuid();
    private readonly Guid _userPublicId = Guid.NewGuid();

    public ProductCategoriesControllerIntegrationTests(TransactionWebApplicationFactory<Program> factory)
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
    public async Task GetList_ShouldReturn200AndPayload()
    {
        await SeedCategoriesAsync(
            new ProductCategory
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = _tenantPublicId,
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductCategory
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = _tenantPublicId,
                Name = "Dessert",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        var response = await _client.GetAsync("/api/product-categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductCategoryDto>>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.GetListSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Should().HaveCount(2);
        body.Data[0].Name.Should().Be("Coffee");
        body.Data[1].Name.Should().Be("Dessert");
    }

    [Fact]
    public async Task Create_ShouldReturn200_WhenRequestIsValid()
    {
        var request = new CreateProductCategoryRequest("Coffee", 1);

        var response = await _client.PostAsJsonAsync("/api/product-categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductCategoryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.CreateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Coffee");
        body.Data.DisplayOrder.Should().Be(1);
        body.Data.IsActive.Should().BeTrue();

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var saved = await db.ProductCategories.SingleAsync(x => x.Name == "Coffee");
        saved.TenantPublicId.Should().Be(_tenantPublicId);
        saved.DisplayOrder.Should().Be(1);
        saved.IsActive.Should().BeTrue();
        saved.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        var request = new CreateProductCategoryRequest("", 1);

        var response = await _client.PostAsJsonAsync("/api/product-categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductCategoryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.CreateParameterError);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameAlreadyExists()
    {
        await SeedCategoriesAsync(
            new ProductCategory
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = _tenantPublicId,
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        var request = new CreateProductCategoryRequest("Coffee", 2);

        var response = await _client.PostAsJsonAsync("/api/product-categories", request);
        var raw = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, raw);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductCategoryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.CreateDuplicatedName);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldReturn200_WhenRequestIsValid()
    {
        var publicId = Guid.NewGuid();

        await SeedCategoriesAsync(
            new ProductCategory
            {
                PublicId = publicId,
                TenantPublicId = _tenantPublicId,
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        var request = new UpdateProductCategoryRequest("Updated Coffee", 3, false);

        var response = await _client.PutAsJsonAsync($"/api/product-categories/{publicId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductCategoryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.UpdateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Updated Coffee");
        body.Data.DisplayOrder.Should().Be(3);
        body.Data.IsActive.Should().BeFalse();

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var updated = await db.ProductCategories.SingleAsync(x => x.PublicId == publicId);
        updated.Name.Should().Be("Updated Coffee");
        updated.DisplayOrder.Should().Be(3);
        updated.IsActive.Should().BeFalse();
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenCategoryNotFound()
    {
        var request = new UpdateProductCategoryRequest("Coffee", 3, true);

        var response = await _client.PutAsJsonAsync($"/api/product-categories/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductCategoryDto>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.NotFound);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturn200_WhenCategoryExistsAndHasNoProducts()
    {
        var publicId = Guid.NewGuid();

        await SeedCategoriesAsync(
            new ProductCategory
            {
                PublicId = publicId,
                TenantPublicId = _tenantPublicId,
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        var response = await _client.DeleteAsync($"/api/product-categories/{publicId}");
        var raw = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, raw);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.DeleteSuccess);
        body.Data.Should().BeTrue();

        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var deleted = await db.ProductCategories
            .IgnoreQueryFilters()
            .SingleAsync(x => x.PublicId == publicId);

        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturn400_WhenCategoryNotFound()
    {
        var response = await _client.DeleteAsync($"/api/product-categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Code.Should().Be(ResultCodes.ProductCategory.NotFound);
    }

    private async Task SeedCategoriesAsync(params ProductCategory[] categories)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        db.ProductCategories.AddRange(categories);
        await db.SaveChangesAsync();
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
}