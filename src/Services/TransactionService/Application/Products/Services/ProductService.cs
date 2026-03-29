using SharedKernel.Common.Results;
using SharedKernel.Contracts.AuditLogs;
using SharedKernel.Topics;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Dtos;
using TransactionService.Application.Products.Queries;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Products.Services;

public class ProductService(
    IProductRepository productRepository,
    ICurrentTenantContext currentTenantContext,
    IUnitOfWork unitOfWork,
    IAuditLogPublisher auditLogPublisher)
    : IProductService
{
    public async Task<ServiceResult<ProductDto>> CreateAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.CreateParameterError,
                "Product name is required.");
        }

        if (request.Price < 0)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.CreateParameterError,
                "Price must be greater than or equal to 0.");
        }

        var category = await productRepository.GetCategoryByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.CategoryPublicId,
            cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.CategoryNotFound,
                "Category not found.");
        }

        var exists = await productRepository.ExistsByNameAsync(
            currentTenantContext.TenantPublicId,
            category.Id,
            name,
            cancellationToken);

        if (exists)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.CreateDuplicatedName,
                "Product name already exists in this category.");
        }

        var product = new Product
        {
            TenantPublicId = currentTenantContext.TenantPublicId,
            CategoryId = category.Id,
            Name = name,
            Description = request.Description?.Trim(),
            Price = request.Price,
            ImageUrl = request.ImageUrl?.Trim(),
            DisplayOrder = request.DisplayOrder ?? 999,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        product.Category = category;

        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MenuItemCreated,
            new AuditLogMessage
            {
                TenantPublicId = currentTenantContext.TenantPublicId.ToString(),
                ActorUserPublicId = currentTenantContext.UserPublicId.ToString(),
                ActorDisplayName = currentTenantContext.UserName,

                ActionType = "Created",
                Category = "MenuItem",

                TargetType = "MenuItem",
                TargetPublicId = product.PublicId.ToString(),
                TargetDisplay = product.Name,

                Description = $"{currentTenantContext.UserName} created menu item {product.Name}",

                Source = "TransactionService",

                Metadata =
                [
                    new AuditMetadataItem("CategoryName", category.Name),
                    new AuditMetadataItem("Price", product.Price.ToString("F2")),
                    new AuditMetadataItem("DisplayOrder", product.DisplayOrder.ToString()),
                    new AuditMetadataItem("IsAvailable", product.IsAvailable.ToString())
                ]
            },
            cancellationToken);
        return ServiceResult<ProductDto>.Ok(
            MapToDto(product),
            ResultCodes.Product.CreateSuccess,
            "Product created successfully.");
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var product = await productRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.PublicId,
            cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.NotFound,
                "Product not found.");
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.UpdateParameterError,
                "Product name is required.");
        }

        if (request.Price < 0)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.UpdateParameterError,
                "Price must be greater than or equal to 0.");
        }

        var category = await productRepository.GetCategoryByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.CategoryPublicId,
            cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.CategoryNotFound,
                "Category not found.");
        }

        var duplicated = await productRepository.ExistsByNameExceptAsync(
            currentTenantContext.TenantPublicId,
            category.Id,
            name,
            request.PublicId,
            cancellationToken);

        if (duplicated)
        {
            return ServiceResult<ProductDto>.Fail(
                ResultCodes.Product.UpdateDuplicatedName,
                "Product name already exists in this category.");
        }

        product.CategoryId = category.Id;
        product.Name = name;
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.DisplayOrder = request.DisplayOrder ?? product.DisplayOrder;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;
        product.Category = category;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MenuItemUpdated,
            new AuditLogMessage
            {
                TenantPublicId = currentTenantContext.TenantPublicId.ToString(),
                ActorUserPublicId = currentTenantContext.UserPublicId.ToString(),
                ActorDisplayName = currentTenantContext.UserName,

                ActionType = "Updated",
                Category = "MenuItem",

                TargetType = "MenuItem",
                TargetPublicId = product.PublicId.ToString(),
                TargetDisplay = product.Name,

                Description = $"{currentTenantContext.UserName} updated menu item {product.Name}",

                Source = "TransactionService",

                Metadata =
                [
                    new AuditMetadataItem("CategoryName", category.Name),
                    new AuditMetadataItem("Price", product.Price.ToString("F2")),
                    new AuditMetadataItem("DisplayOrder", product.DisplayOrder.ToString()),
                    new AuditMetadataItem("IsAvailable", product.IsAvailable.ToString())
                ]
            },
            cancellationToken);
        return ServiceResult<ProductDto>.Ok(
            MapToDto(product),
            ResultCodes.Product.UpdateSuccess,
            "Product updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var product = await productRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.PublicId,
            cancellationToken);

        if (product is null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Product.NotFound,
                "Product not found.");
        }

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MenuItemDeleted,
            new AuditLogMessage
            {
                TenantPublicId = currentTenantContext.TenantPublicId.ToString(),
                ActorUserPublicId = currentTenantContext.UserPublicId.ToString(),
                ActorDisplayName = currentTenantContext.UserName,

                ActionType = "Deleted",
                Category = "MenuItem",

                TargetType = "MenuItem",
                TargetPublicId = product.PublicId.ToString(),
                TargetDisplay = product.Name,

                Description = $"{currentTenantContext.UserName} deleted menu item {product.Name}",

                Source = "TransactionService",

                Metadata =
                [
                    new AuditMetadataItem("CategoryName", product.Category.Name),
                    new AuditMetadataItem("Price", product.Price.ToString("F2"))
                ]
            },
            cancellationToken);
        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.Product.DeleteSuccess,
            "Product deleted successfully.");
    }

    public async Task<ServiceResult<List<ProductDto>>> GetByCategoryAsync(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<List<ProductDto>>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var products = await productRepository.GetByCategoryPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.CategoryPublicId,
            cancellationToken);

        var result = products.Select(MapToDto).ToList();

        return ServiceResult<List<ProductDto>>.Ok(
            result,
            ResultCodes.Product.GetListSuccess,
            "Products retrieved successfully.");
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            PublicId = product.PublicId,
            CategoryPublicId = product.Category.PublicId,
            CategoryName = product.Category.Name,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            DisplayOrder = product.DisplayOrder,
            IsAvailable = product.IsAvailable
        };
    }
}