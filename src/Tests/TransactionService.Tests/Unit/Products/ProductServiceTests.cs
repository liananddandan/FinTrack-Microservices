using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.AuditLogs;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Queries;
using TransactionService.Application.Products.Services;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.Tests.Unit.Products;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICurrentTenantContext> _currentTenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProductService _service;
    private readonly Mock<IAuditLogPublisher> _auditLogPublisherMock;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _currentTenantContextMock = new Mock<ICurrentTenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditLogPublisherMock = new Mock<IAuditLogPublisher>();
        _service = new ProductService(
            _productRepositoryMock.Object,
            _currentTenantContextMock.Object,
            _unitOfWorkMock.Object,
            _auditLogPublisherMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenTenantContextMissing()
    {
        var command = new CreateProductCommand(
            Guid.NewGuid(),
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(Guid.Empty);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Forbidden);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateProductCommand(
            Guid.NewGuid(),
            " ",
            null,
            5.5m,
            null,
            1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(Guid.NewGuid());

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Product.CreateParameterError);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenPriceIsNegative()
    {
        var command = new CreateProductCommand(
            Guid.NewGuid(),
            "Flat White",
            null,
            -1m,
            null,
            1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(Guid.NewGuid());

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Product.CreateParameterError);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenCategoryNotFound()
    {
        var tenantPublicId = Guid.NewGuid();
        var categoryPublicId = Guid.NewGuid();

        var command = new CreateProductCommand(
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetCategoryByPublicIdAsync(
                tenantPublicId,
                categoryPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Product.CategoryNotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenDuplicatedNameExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var categoryPublicId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = 10,
            PublicId = categoryPublicId,
            TenantPublicId = tenantPublicId,
            Name = "Coffee",
            DisplayOrder = 1,
            IsActive = true
        };

        var command = new CreateProductCommand(
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetCategoryByPublicIdAsync(
                tenantPublicId,
                categoryPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.ExistsByNameAsync(
                tenantPublicId,
                category.Id,
                "Flat White",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Product.CreateDuplicatedName);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct_WhenRequestIsValid()
    {
        var tenantPublicId = Guid.NewGuid();
        var categoryPublicId = Guid.NewGuid();

        var category = new ProductCategory
        {
            Id = 10,
            PublicId = categoryPublicId,
            TenantPublicId = tenantPublicId,
            Name = "Coffee",
            DisplayOrder = 1,
            IsActive = true
        };

        var command = new CreateProductCommand(
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.5m,
            null,
            2);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetCategoryByPublicIdAsync(
                tenantPublicId,
                categoryPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.ExistsByNameAsync(
                tenantPublicId,
                category.Id,
                "Flat White",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Product? savedEntity = null;

        _productRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((entity, _) => savedEntity = entity)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Product.CreateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Flat White");
        result.Data.Price.Should().Be(5.5m);
        result.Data.CategoryName.Should().Be("Coffee");

        savedEntity.Should().NotBeNull();
        savedEntity!.TenantPublicId.Should().Be(tenantPublicId);
        savedEntity.CategoryId.Should().Be(category.Id);
        savedEntity.DisplayOrder.Should().Be(2);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenProductNotFound()
    {
        var tenantPublicId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1,
            true);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                command.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Product.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenRequestIsValid()
    {
        var tenantPublicId = Guid.NewGuid();
        var categoryPublicId = Guid.NewGuid();

        var oldCategory = new ProductCategory
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Old Category"
        };

        var newCategory = new ProductCategory
        {
            Id = 2,
            PublicId = categoryPublicId,
            TenantPublicId = tenantPublicId,
            Name = "Coffee"
        };

        var product = new Product
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            CategoryId = oldCategory.Id,
            Category = oldCategory,
            Name = "Old Name",
            Price = 4m,
            DisplayOrder = 1,
            IsAvailable = true
        };

        var command = new UpdateProductCommand(
            product.PublicId,
            categoryPublicId,
            "Flat White",
            "Coffee",
            5.5m,
            "https://example.com/flatwhite.png",
            3,
            false);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                product.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock.Setup(x => x.GetCategoryByPublicIdAsync(
                tenantPublicId,
                categoryPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _productRepositoryMock.Setup(x => x.ExistsByNameExceptAsync(
                tenantPublicId,
                newCategory.Id,
                "Flat White",
                product.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Product.UpdateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Flat White");
        result.Data.CategoryName.Should().Be("Coffee");
        result.Data.IsAvailable.Should().BeFalse();

        product.Name.Should().Be("Flat White");
        product.CategoryId.Should().Be(newCategory.Id);
        product.Price.Should().Be(5.5m);
        product.DisplayOrder.Should().Be(3);
        product.IsAvailable.Should().BeFalse();
        product.UpdatedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProduct_WhenProductExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var product = new Product
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Flat White",
            Price = 5.5m,
            Category = new ProductCategory
            {
                Id = 10,
                Name = "Coffee"
            }
        };

        var command = new DeleteProductCommand(product.PublicId);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);
        _currentTenantContextMock.Setup(x => x.UserPublicId).Returns(Guid.NewGuid());
        _currentTenantContextMock.Setup(x => x.UserName).Returns("Test User");
        _productRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                product.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _auditLogPublisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<AuditLogMessage>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var result = await _service.DeleteAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Product.DeleteSuccess);
        result.Data.Should().BeTrue();

        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnProducts_WhenCategoryExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var categoryPublicId = Guid.NewGuid();

        var category = new ProductCategory
        {
            Id = 1,
            PublicId = categoryPublicId,
            TenantPublicId = tenantPublicId,
            Name = "Coffee"
        };

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantPublicId,
                CategoryId = category.Id,
                Category = category,
                Name = "Flat White",
                Price = 5.5m,
                DisplayOrder = 1,
                IsAvailable = true
            },
            new()
            {
                Id = 2,
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantPublicId,
                CategoryId = category.Id,
                Category = category,
                Name = "Latte",
                Price = 5.8m,
                DisplayOrder = 2,
                IsAvailable = true
            }
        };

        var query = new GetProductsByCategoryQuery(categoryPublicId);

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(tenantPublicId);

        _productRepositoryMock.Setup(x => x.GetByCategoryPublicIdAsync(
                tenantPublicId,
                categoryPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _service.GetByCategoryAsync(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Product.GetListSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
        result.Data[0].Name.Should().Be("Flat White");
        result.Data[1].Name.Should().Be("Latte");
    }
}