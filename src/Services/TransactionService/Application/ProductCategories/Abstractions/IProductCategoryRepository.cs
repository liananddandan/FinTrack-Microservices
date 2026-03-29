using TransactionService.Domain.Entities;

namespace TransactionService.Application.ProductCategories.Abstractions;

public interface IProductCategoryRepository
{
    Task<bool> ExistsByNameAsync(
        Guid tenantPublicId,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameExceptAsync(
        Guid tenantPublicId,
        string name,
        Guid excludePublicId,
        CancellationToken cancellationToken);

    Task<ProductCategory?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid publicId,
        CancellationToken cancellationToken);

    Task<List<ProductCategory>> GetListAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyProductsAsync(
        long categoryId,
        CancellationToken cancellationToken);

    Task AddAsync(
        ProductCategory category,
        CancellationToken cancellationToken);
    
    Task<ProductCategory?> GetByNameAsync(
        Guid tenantPublicId,
        string name,
        CancellationToken cancellationToken);

}