using Microsoft.EntityFrameworkCore;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class OrderRepository(TransactionDbContext dbContext) : IOrderRepository
{
    public Task<List<Product>> GetProductsByPublicIdsAsync(
        Guid tenantPublicId,
        List<Guid> productPublicIds,
        CancellationToken cancellationToken)
    {
        return dbContext.Products
            .Include(x => x.Category)
            .Where(x =>
                x.TenantPublicId == tenantPublicId &&
                productPublicIds.Contains(x.PublicId) &&
                x.IsAvailable)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task<Order?> GetByPublicIdAsync(
        Guid tenantPublicId,
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId && x.PublicId == orderPublicId,
                cancellationToken);
    }

    public async Task<PagedResult<OrderListItemDto>> GetPagedAsync(
        Guid tenantPublicId,
        Guid? createdByUserPublicId,
        string? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId);

        if (createdByUserPublicId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserPublicId == createdByUserPublicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderListItemDto
            {
                PublicId = x.PublicId,
                OrderNumber = x.OrderNumber,
                CustomerName = x.CustomerName,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                PaymentStatus = x.PaymentStatus,
                PaymentMethod = x.PaymentMethod,
                CreatedByUserNameSnapshot = x.CreatedByUserNameSnapshot,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task<int> CountTodayOrdersAsync(
        Guid tenantPublicId,
        DateTime utcDate,
        CancellationToken cancellationToken)
    {
        var dayStart = utcDate.Date;
        var dayEnd = dayStart.AddDays(1);

        return dbContext.Orders.CountAsync(
            x => x.TenantPublicId == tenantPublicId &&
                 x.CreatedAt >= dayStart &&
                 x.CreatedAt < dayEnd,
            cancellationToken);
    }

    public async Task<OrderSummaryDto> GetSummaryAsync(
        Guid tenantPublicId,
        Guid? createdByUserPublicId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId);

        if (createdByUserPublicId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserPublicId == createdByUserPublicId.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= toUtc.Value);
        }

        var completedOrders = query.Where(x => x.Status == OrderStatuses.Completed);
        var cancelledOrders = query.Where(x => x.Status == OrderStatuses.Cancelled);

        var orderCount = await completedOrders.CountAsync(cancellationToken);
        var totalRevenue = orderCount == 0
            ? 0m
            : await completedOrders.SumAsync(x => x.TotalAmount, cancellationToken);

        var cancelledOrderCount = await cancelledOrders.CountAsync(cancellationToken);

        return new OrderSummaryDto
        {
            OrderCount = orderCount,
            TotalRevenue = totalRevenue,
            AverageOrderValue = orderCount == 0 ? 0m : totalRevenue / orderCount,
            CancelledOrderCount = cancelledOrderCount
        };
    }
}