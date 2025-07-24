using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Commands;
using TransactionService.Common.Extensions;
using TransactionService.Common.Requests;

namespace TransactionService.Controllers;

[ApiController]
[Route("api/transaction")]
public class TransactionController(IMediator mediator) : BaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand(
            TenantPublicId, 
            UserPublicId, 
            request.Amount,
            request.Currency,
            request.Description);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("{transactionPublicId}")]
    public async Task<IActionResult> GetTransactionsAsync([FromRoute] string transactionPublicId)
    {
        var command = new QueryTransactionCommand(TenantPublicId, UserPublicId, transactionPublicId);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}