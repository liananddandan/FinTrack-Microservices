using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Commands;

namespace TransactionService.Application.ProductCategories.Handlers;

public class UpdateProductCategoryCommandHandler(IProductCategoryService productCategoryService)
    : IRequestHandler<UpdateProductCategoryCommand, ServiceResult<ProductCategoryDto>>
{
    public Task<ServiceResult<ProductCategoryDto>> Handle(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        return productCategoryService.UpdateAsync(request, cancellationToken);
    }
}