using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Commands;

namespace TransactionService.Application.Products.Handlers;


public class DeleteProductCommandHandler(IProductService productService)
    : IRequestHandler<DeleteProductCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        return productService.DeleteAsync(request, cancellationToken);
    }
}