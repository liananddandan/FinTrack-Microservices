using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.ProductCategories.Queries;

public record GetProductCategoriesQuery()
    : IRequest<ServiceResult<List<ProductCategoryDto>>>;