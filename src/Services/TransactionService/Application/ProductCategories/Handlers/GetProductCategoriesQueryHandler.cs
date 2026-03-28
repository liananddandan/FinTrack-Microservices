using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Queries;

namespace TransactionService.Application.ProductCategories.Handlers;

public class GetProductCategoriesQueryHandler(IProductCategoryService productCategoryService)
    : IRequestHandler<GetProductCategoriesQuery, ServiceResult<List<ProductCategoryDto>>>
{
    public Task<ServiceResult<List<ProductCategoryDto>>> Handle(
        GetProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return productCategoryService.GetListAsync(cancellationToken);
    }
}