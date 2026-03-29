using TransactionService.Domain.Entities;

namespace TransactionService.Application.Products.Abstractions;

public interface IProductRepository
{
    Task<ProductCategory?> GetCategoryByPublicIdAsync(
        Guid tenantPublicId,
        Guid categoryPublicId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        Guid tenantPublicId,
        long categoryId,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameExceptAsync(
        Guid tenantPublicId,
        long categoryId,
        string name,
        Guid excludeProductPublicId,
        CancellationToken cancellationToken);

    Task<Product?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid productPublicId,
        CancellationToken cancellationToken);

    Task<List<Product>> GetByCategoryPublicIdAsync(
        Guid tenantPublicId,
        Guid categoryPublicId,
        CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
    
    Task<Product?> GetByNameAsync(
        Guid tenantPublicId,
        string name,
        CancellationToken cancellationToken);
}