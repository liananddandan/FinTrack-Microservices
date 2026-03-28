using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Commands;
using TransactionService.Application.ProductCategories.Services;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.Tests.Unit.ProductCategories;

public class ProductCategoryServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IProductCategoryRepository> _repositoryMock;
    private readonly Mock<ICurrentTenantContext> _currentTenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProductCategoryService _service;

    public ProductCategoryServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _repositoryMock = new Mock<IProductCategoryRepository>();
        _currentTenantContextMock = new Mock<ICurrentTenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new ProductCategoryService(
            _repositoryMock.Object,
            _currentTenantContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenTenantContextMissing()
    {
        var command = new CreateProductCategoryCommand("Coffee", 1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(Guid.Empty);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Forbidden);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateProductCategoryCommand("   ", 1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(Guid.NewGuid());

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.ProductCategory.CreateParameterError);

        _repositoryMock.Verify(
            x => x.ExistsByNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenNameAlreadyExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var command = new CreateProductCategoryCommand("Coffee", 1);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.ExistsByNameAsync(
                tenantPublicId,
                "Coffee",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.ProductCategory.CreateDuplicatedName);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory_WhenRequestIsValid()
    {
        var tenantPublicId = Guid.NewGuid();
        var command = new CreateProductCategoryCommand("Coffee", 2);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.ExistsByNameAsync(
                tenantPublicId,
                "Coffee",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        ProductCategory? savedEntity = null;

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()))
            .Callback<ProductCategory, CancellationToken>((entity, _) => savedEntity = entity)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.ProductCategory.CreateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Coffee");
        result.Data.DisplayOrder.Should().Be(2);
        result.Data.IsActive.Should().BeTrue();

        savedEntity.Should().NotBeNull();
        savedEntity!.TenantPublicId.Should().Be(tenantPublicId);
        savedEntity.Name.Should().Be("Coffee");

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenCategoryNotFound()
    {
        var tenantPublicId = Guid.NewGuid();
        var command = new UpdateProductCategoryCommand(Guid.NewGuid(), "Coffee", 1, true);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                command.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        var result = await _service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.ProductCategory.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenDuplicatedNameExists()
    {
        var tenantPublicId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Old Name",
            DisplayOrder = 1,
            IsActive = true
        };

        var command = new UpdateProductCategoryCommand(category.PublicId, "Coffee", 3, false);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock.Setup(x => x.ExistsByNameExceptAsync(
                tenantPublicId,
                "Coffee",
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.ProductCategory.UpdateDuplicatedName);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenRequestIsValid()
    {
        var tenantPublicId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Old Name",
            DisplayOrder = 1,
            IsActive = true
        };

        var command = new UpdateProductCategoryCommand(category.PublicId, "New Name", 9, false);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock.Setup(x => x.ExistsByNameExceptAsync(
                tenantPublicId,
                "New Name",
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.ProductCategory.UpdateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Name");
        result.Data.DisplayOrder.Should().Be(9);
        result.Data.IsActive.Should().BeFalse();

        category.Name.Should().Be("New Name");
        category.DisplayOrder.Should().Be(9);
        category.IsActive.Should().BeFalse();
        category.UpdatedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenCategoryContainsProducts()
    {
        var tenantPublicId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = 123,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Coffee"
        };

        var command = new DeleteProductCategoryCommand(category.PublicId);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock.Setup(x => x.HasAnyProductsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.ProductCategory.DeleteHasProducts);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteCategory_WhenNoProductsExist()
    {
        var tenantPublicId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = 123,
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = "Coffee",
            IsDeleted = false
        };

        var command = new DeleteProductCategoryCommand(category.PublicId);

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetByPublicIdAsync(
                tenantPublicId,
                category.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock.Setup(x => x.HasAnyProductsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.ProductCategory.DeleteSuccess);
        result.Data.Should().BeTrue();

        category.IsDeleted.Should().BeTrue();
        category.DeletedAt.Should().NotBeNull();
        category.UpdatedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnMappedCategories()
    {
        var tenantPublicId = Guid.NewGuid();

        var categories = new List<ProductCategory>
        {
            new()
            {
                Id = 1,
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantPublicId,
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true
            },
            new()
            {
                Id = 2,
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantPublicId,
                Name = "Dessert",
                DisplayOrder = 2,
                IsActive = false
            }
        };

        _currentTenantContextMock.Setup(x => x.TenantPublicId)
            .Returns(tenantPublicId);

        _repositoryMock.Setup(x => x.GetListAsync(
                tenantPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _service.GetListAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.ProductCategory.GetListSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(2);
        result.Data[0].Name.Should().Be("Coffee");
        result.Data[1].Name.Should().Be("Dessert");
    }
}