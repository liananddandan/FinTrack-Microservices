using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Products.Dtos;

namespace TransactionService.Application.Products.Queries;

public record GetProductsByCategoryQuery(
    Guid CategoryPublicId
) : IRequest<ServiceResult<List<ProductDto>>>;