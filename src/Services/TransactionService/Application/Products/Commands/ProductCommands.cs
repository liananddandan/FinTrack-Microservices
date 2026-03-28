using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Dtos;

namespace TransactionService.Application.Products.Commands;

public record CreateProductCommand(
    Guid CategoryPublicId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int? DisplayOrder
) : IRequest<ServiceResult<ProductDto>>;

public record UpdateProductCommand(
    Guid PublicId,
    Guid CategoryPublicId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int? DisplayOrder,
    bool IsAvailable
) : IRequest<ServiceResult<ProductDto>>;

public record DeleteProductCommand(
    Guid PublicId
) : IRequest<ServiceResult<bool>>;