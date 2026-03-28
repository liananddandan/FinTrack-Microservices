using Microsoft.EntityFrameworkCore;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class ProductCategoryRepository(TransactionDbContext dbContext) : IProductCategoryRepository
{
    public Task<bool> ExistsByNameAsync(
        Guid tenantPublicId,
        string name,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductCategories
            .AnyAsync(x => x.TenantPublicId == tenantPublicId && x.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameExceptAsync(
        Guid tenantPublicId,
        string name,
        Guid excludePublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductCategories
            .AnyAsync(x =>
                x.TenantPublicId == tenantPublicId &&
                x.Name == name &&
                x.PublicId != excludePublicId,
                cancellationToken);
    }

    public Task<ProductCategory?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid publicId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductCategories
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId && x.PublicId == publicId,
                cancellationToken);
    }

    public Task<List<ProductCategory>> GetListAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductCategories
            .Where(x => x.TenantPublicId == tenantPublicId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasAnyProductsAsync(
        long categoryId,
        CancellationToken cancellationToken)
    {
        return dbContext.Products
            .AnyAsync(x => x.CategoryId == categoryId, cancellationToken);
    }

    public async Task AddAsync(
        ProductCategory category,
        CancellationToken cancellationToken)
    {
        await dbContext.ProductCategories.AddAsync(category, cancellationToken);
    }
}