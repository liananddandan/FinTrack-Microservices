using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Dtos;

namespace TransactionService.Application.Products.Handlers;


public class UpdateProductCommandHandler(IProductService productService)
    : IRequestHandler<UpdateProductCommand, ServiceResult<ProductDto>>
{
    public Task<ServiceResult<ProductDto>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        return productService.UpdateAsync(request, cancellationToken);
    }
}