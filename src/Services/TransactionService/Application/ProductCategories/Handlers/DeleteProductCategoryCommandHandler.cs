using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.ProductCategories.Commands;

namespace TransactionService.Application.ProductCategories.Handlers;

public class DeleteProductCategoryCommandHandler(IProductCategoryService productCategoryService)
    : IRequestHandler<DeleteProductCategoryCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        DeleteProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        return productCategoryService.DeleteAsync(request, cancellationToken);
    }
}