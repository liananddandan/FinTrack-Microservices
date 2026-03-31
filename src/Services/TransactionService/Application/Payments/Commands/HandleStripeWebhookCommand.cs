using MediatR;
using SharedKernel.Common.Results;
using Stripe;

namespace TransactionService.Application.Payments.Commands;

public record HandleStripeWebhookCommand(
    Event StripeEvent) : IRequest<ServiceResult<bool>>;