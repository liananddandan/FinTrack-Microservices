using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Commands;

public record CreatePaymentCommand(
    Guid OrderPublicId,
    string Provider,
    string PaymentMethod) : IRequest<ServiceResult<PaymentDto>>;