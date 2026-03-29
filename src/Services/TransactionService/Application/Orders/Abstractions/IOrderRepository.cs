using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Orders.Abstractions;

public interface IOrderRepository
{
    Task<List<Product>> GetProductsByPublicIdsAsync(
        Guid tenantPublicId,
        List<Guid> productPublicIds,
        CancellationToken cancellationToken);

    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid orderPublicId,
        CancellationToken cancellationToken);

    Task<PagedResult<OrderListItemDto>> GetPagedAsync(
        Guid tenantPublicId,
        Guid? createdByUserPublicId,
        string? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountTodayOrdersAsync(
        Guid tenantPublicId,
        DateTime utcDate,
        CancellationToken cancellationToken);
    
    Task<OrderSummaryDto> GetSummaryAsync(
        Guid tenantPublicId,
        Guid? createdByUserPublicId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken);
}