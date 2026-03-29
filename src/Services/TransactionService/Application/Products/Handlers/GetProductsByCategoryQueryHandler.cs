using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Dtos;
using TransactionService.Application.Products.Queries;

namespace TransactionService.Application.Products.Handlers;

public class GetProductsByCategoryQueryHandler(IProductService productService)
    : IRequestHandler<GetProductsByCategoryQuery, ServiceResult<List<ProductDto>>>
{
    public Task<ServiceResult<List<ProductDto>>> Handle(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        return productService.GetByCategoryAsync(request, cancellationToken);
    }
}