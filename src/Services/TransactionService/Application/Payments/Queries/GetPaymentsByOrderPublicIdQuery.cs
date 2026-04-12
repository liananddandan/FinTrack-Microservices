using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Queries;

public sealed record GetPaymentsByOrderPublicIdQuery(
    string OrderPublicId)
    : IRequest<ServiceResult<List<PaymentListItemDto>>>;