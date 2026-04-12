using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentService
{
    Task<ServiceResult<CreatePaymentResultDto>> CreatePaymentAsync(
        string orderPublicId,
        string paymentMethodType,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PaymentDetailDto>> GetPaymentByPublicIdAsync(
        string paymentPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<PaymentListItemDto>>> GetPaymentsByOrderPublicIdAsync(
        string orderPublicId,
        CancellationToken cancellationToken = default);
}