using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Queries;

public sealed record GetPaymentByPublicIdQuery(
    string PaymentPublicId)
    : IRequest<ServiceResult<PaymentDetailDto>>;