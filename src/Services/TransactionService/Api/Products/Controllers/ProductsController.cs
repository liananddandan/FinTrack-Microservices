using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.Products.Contracts;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Products.Commands;

namespace TransactionService.Api.Products.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.CategoryPublicId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.DisplayOrder);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> Update(
        Guid publicId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            publicId,
            request.CategoryPublicId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.DisplayOrder,
            request.IsAvailable);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Delete(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(publicId);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}