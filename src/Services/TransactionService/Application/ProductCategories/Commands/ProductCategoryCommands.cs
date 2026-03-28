using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.ProductCategories.Commands;


public record CreateProductCategoryCommand(
    string Name,
    int DisplayOrder
) : IRequest<ServiceResult<ProductCategoryDto>>;

public record UpdateProductCategoryCommand(
    Guid PublicId,
    string Name,
    int DisplayOrder,
    bool IsActive
) : IRequest<ServiceResult<ProductCategoryDto>>;

public record DeleteProductCategoryCommand(
    Guid PublicId
) : IRequest<ServiceResult<bool>>;