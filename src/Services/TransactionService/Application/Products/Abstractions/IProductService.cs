using SharedKernel.Common.Results;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Dtos;
using TransactionService.Application.Products.Queries;

namespace TransactionService.Application.Products.Abstractions;

public interface IProductService
{
    Task<ServiceResult<ProductDto>> CreateAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductDto>> UpdateAsync(
        UpdateProductCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> DeleteAsync(
        DeleteProductCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<List<ProductDto>>> GetByCategoryAsync(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken);
}