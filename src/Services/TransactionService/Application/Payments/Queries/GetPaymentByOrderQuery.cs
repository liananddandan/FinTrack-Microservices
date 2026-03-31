using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Queries;

public record GetPaymentByOrderQuery(Guid OrderPublicId) : IRequest<ServiceResult<PaymentDto>>;