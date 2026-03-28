using SharedKernel.Common.Results;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Commands;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.ProductCategories.Services;

public class ProductCategoryService(
    IProductCategoryRepository productCategoryRepository,
    ICurrentTenantContext currentTenantContext,
    IUnitOfWork unitOfWork)
    : IProductCategoryService
{
    public async Task<ServiceResult<ProductCategoryDto>> CreateAsync(
        CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.ProductCategory.CreateParameterError,
                "Category name is required.");
        }

        var exists = await productCategoryRepository.ExistsByNameAsync(
            currentTenantContext.TenantPublicId,
            name,
            cancellationToken);

        if (exists)
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.ProductCategory.CreateDuplicatedName,
                "Category name already exists.");
        }

        var category = new ProductCategory
        {
            TenantPublicId = currentTenantContext.TenantPublicId,
            Name = name,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await productCategoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductCategoryDto>.Ok(
            MapToDto(category),
            ResultCodes.ProductCategory.CreateSuccess,
            "Category created successfully.");
    }

    public async Task<ServiceResult<ProductCategoryDto>> UpdateAsync(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var category = await productCategoryRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.PublicId,
            cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.ProductCategory.NotFound,
                "Category not found.");
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.ProductCategory.UpdateParameterError,
                "Category name is required.");
        }

        var duplicated = await productCategoryRepository.ExistsByNameExceptAsync(
            currentTenantContext.TenantPublicId,
            name,
            request.PublicId,
            cancellationToken);

        if (duplicated)
        {
            return ServiceResult<ProductCategoryDto>.Fail(
                ResultCodes.ProductCategory.UpdateDuplicatedName,
                "Category name already exists.");
        }

        category.Name = name;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductCategoryDto>.Ok(
            MapToDto(category),
            ResultCodes.ProductCategory.UpdateSuccess,
            "Category updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        DeleteProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var category = await productCategoryRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.PublicId,
            cancellationToken);

        if (category is null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.ProductCategory.NotFound,
                "Category not found.");
        }

        var hasProducts = await productCategoryRepository.HasAnyProductsAsync(
            category.Id,
            cancellationToken);

        if (hasProducts)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.ProductCategory.DeleteHasProducts,
                "Category cannot be deleted because it already contains products.");
        }

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.ProductCategory.DeleteSuccess,
            "Category deleted successfully.");
    }

    public async Task<ServiceResult<List<ProductCategoryDto>>> GetListAsync(
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<List<ProductCategoryDto>>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var categories = await productCategoryRepository.GetListAsync(
            currentTenantContext.TenantPublicId,
            cancellationToken);

        var result = categories
            .Select(MapToDto)
            .ToList();

        return ServiceResult<List<ProductCategoryDto>>.Ok(
            result,
            ResultCodes.ProductCategory.GetListSuccess,
            "Product categories retrieved successfully.");
    }

    private static ProductCategoryDto MapToDto(ProductCategory category)
    {
        return new ProductCategoryDto
        {
            PublicId = category.PublicId,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }
}