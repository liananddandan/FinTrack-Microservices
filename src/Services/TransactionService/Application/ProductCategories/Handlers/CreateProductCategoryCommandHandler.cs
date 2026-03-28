using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Commands;

namespace TransactionService.Application.ProductCategories.Handlers;

public class CreateProductCategoryCommandHandler(IProductCategoryService productCategoryService)
    : IRequestHandler<CreateProductCategoryCommand, ServiceResult<ProductCategoryDto>>
{
    public Task<ServiceResult<ProductCategoryDto>> Handle(
        CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        return productCategoryService.CreateAsync(request, cancellationToken);
    }
}