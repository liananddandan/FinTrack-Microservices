using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.ProductCategories.Contracts;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.ProductCategories.Commands;
using TransactionService.Application.ProductCategories.Queries;

namespace TransactionService.Api.ProductCategories.Controllers;

[ApiController]
[Route("api/product-categories")]
[Authorize]
public class ProductCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductCategoriesQuery(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCategoryCommand(
            request.Name,
            request.DisplayOrder);

        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> Update(
        Guid publicId,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCategoryCommand(
            publicId,
            request.Name,
            request.DisplayOrder,
            request.IsActive);

        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Delete(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCategoryCommand(publicId);

        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}