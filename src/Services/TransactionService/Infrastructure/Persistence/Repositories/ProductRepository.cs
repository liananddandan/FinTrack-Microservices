using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class ProductRepository(TransactionDbContext dbContext) : IProductRepository
{
    public Task<ProductCategory?> GetCategoryByPublicIdAsync(
        Guid tenantPublicId,
        Guid categoryPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductCategories
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId && x.PublicId == categoryPublicId,
                cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        Guid tenantPublicId,
        long categoryId,
        string name,
        CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            x => x.TenantPublicId == tenantPublicId
                 && x.CategoryId == categoryId
                 && x.Name == name,
            cancellationToken);
    }

    public Task<bool> ExistsByNameExceptAsync(
        Guid tenantPublicId,
        long categoryId,
        string name,
        Guid excludeProductPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            x => x.TenantPublicId == tenantPublicId
                 && x.CategoryId == categoryId
                 && x.Name == name
                 && x.PublicId != excludeProductPublicId,
            cancellationToken);
    }

    public Task<Product?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid productPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId && x.PublicId == productPublicId,
                cancellationToken);
    }

    public Task<List<Product>> GetByCategoryPublicIdAsync(
        Guid tenantPublicId,
        Guid categoryPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.Products
            .Include(x => x.Category)
            .Where(x =>
                x.TenantPublicId == tenantPublicId &&
                x.Category.PublicId == categoryPublicId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> GetByNameAsync(Guid tenantPublicId, string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        return await dbContext.Products
            .FirstOrDefaultAsync(x =>
                x.TenantPublicId == tenantPublicId &&
                x.Name == normalizedName,
                cancellationToken);
    }
}