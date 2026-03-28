using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Commands;

namespace TransactionService.Application.ProductCategories.Abstractions;

public interface IProductCategoryService
{
    Task<ServiceResult<ProductCategoryDto>> CreateAsync(
        CreateProductCategoryCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductCategoryDto>> UpdateAsync(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> DeleteAsync(
        DeleteProductCategoryCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<List<ProductCategoryDto>>> GetListAsync(
        CancellationToken cancellationToken);
}