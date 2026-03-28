using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Dtos;

namespace TransactionService.Application.Products.Handlers;

public class CreateProductCommandHandler(IProductService productService)
    : IRequestHandler<CreateProductCommand, ServiceResult<ProductDto>>
{
    public Task<ServiceResult<ProductDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        return productService.CreateAsync(request, cancellationToken);
    }
}