using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Commands;

public sealed record CreatePaymentCommand(
    string OrderPublicId,
    string PaymentMethodType)
    : IRequest<ServiceResult<CreatePaymentResultDto>>;