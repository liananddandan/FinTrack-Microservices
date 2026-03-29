using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Orders.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ICurrentTenantContext currentTenantContext,
    IUnitOfWork unitOfWork)
    : IOrderService
{
    public async Task<ServiceResult<OrderDto>> CreateAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty ||
            currentTenantContext.UserPublicId == Guid.Empty)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant or user context is missing.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Order.CreateParameterError,
                "Order items are required.");
        }

        var invalidQuantity = request.Items.Any(x => x.Quantity <= 0);
        if (invalidQuantity)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Order.CreateParameterError,
                "Item quantity must be greater than 0.");
        }

        var productIds = request.Items
            .Select(x => x.ProductPublicId)
            .Distinct()
            .ToList();

        var products = await orderRepository.GetProductsByPublicIdsAsync(
            currentTenantContext.TenantPublicId,
            productIds,
            cancellationToken);

        if (products.Count != productIds.Count)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Order.ProductNotFound,
                "One or more products do not exist or are unavailable.");
        }

        var productLookup = products.ToDictionary(x => x.PublicId, x => x);

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0m;

        foreach (var item in request.Items)
        {
            var product = productLookup[item.ProductPublicId];
            var lineTotal = product.Price * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductPublicId = product.PublicId,
                ProductNameSnapshot = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineTotal = lineTotal,
                Notes = item.Notes?.Trim()
            });

            subtotal += lineTotal;
        }

        var gstRate = 0.15m;
        var gstAmount = Math.Round(subtotal * gstRate, 2, MidpointRounding.AwayFromZero);
        var discountAmount = 0m;
        var totalAmount = subtotal + gstAmount - discountAmount;

        var todayCount = await orderRepository.CountTodayOrdersAsync(
            currentTenantContext.TenantPublicId,
            DateTime.UtcNow,
            cancellationToken);

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{(todayCount + 1):D4}";

        var order = new Order
        {
            TenantPublicId = currentTenantContext.TenantPublicId,
            OrderNumber = orderNumber,
            CustomerName = request.CustomerName?.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            CreatedByUserPublicId = currentTenantContext.UserPublicId,
            CreatedByUserNameSnapshot = currentTenantContext.UserName ?? currentTenantContext.UserEmail ?? "Unknown",
            SubtotalAmount = subtotal,
            GstRate = gstRate,
            GstAmount = gstAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Status = OrderStatuses.Completed,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = request.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
            Items = orderItems
        };

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<OrderDto>.Ok(
            MapToDto(order),
            ResultCodes.Order.CreateSuccess,
            "Order created successfully.");
    }

    public async Task<ServiceResult<OrderDto>> GetByPublicIdAsync(
        GetOrderByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var order = await orderRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.OrderPublicId,
            cancellationToken);

        if (order is null)
        {
            return ServiceResult<OrderDto>.Fail(
                ResultCodes.Order.NotFound,
                "Order not found.");
        }

        return ServiceResult<OrderDto>.Ok(
            MapToDto(order),
            ResultCodes.Order.GetByIdSuccess,
            "Order retrieved successfully.");
    }

    public async Task<ServiceResult<PagedResult<OrderListItemDto>>> GetPagedAsync(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<PagedResult<OrderListItemDto>>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        Guid? createdByUserPublicId = request.CreatedByMe
            ? currentTenantContext.UserPublicId
            : null;

        var result = await orderRepository.GetPagedAsync(
            currentTenantContext.TenantPublicId,
            createdByUserPublicId,
            request.Status,
            request.FromUtc,
            request.ToUtc,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return ServiceResult<PagedResult<OrderListItemDto>>.Ok(
            result,
            ResultCodes.Order.GetPagedSuccess,
            "Orders retrieved successfully.");
    }

    public async Task<ServiceResult<bool>> CancelAsync(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var order = await orderRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            request.OrderPublicId,
            cancellationToken);

        if (order is null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Order.NotFound,
                "Order not found.");
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Order.AlreadyCancelled,
                "Order is already cancelled.");
        }

        order.Status = OrderStatuses.Cancelled;
        order.PaidAt = null;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.Order.CancelSuccess,
            "Order cancelled successfully.");
    }

    public async Task<ServiceResult<OrderSummaryDto>> GetSummaryAsync(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<OrderSummaryDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        Guid? createdByUserPublicId = null;

        if (request.CreatedByMe)
        {
            createdByUserPublicId = currentTenantContext.UserPublicId;
        }
        else if (!string.Equals(currentTenantContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            createdByUserPublicId = currentTenantContext.UserPublicId;
        }

        var result = await orderRepository.GetSummaryAsync(
            currentTenantContext.TenantPublicId,
            createdByUserPublicId,
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        return ServiceResult<OrderSummaryDto>.Ok(
            result,
            ResultCodes.Order.GetSummarySuccess,
            "Order summary retrieved successfully.");
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            PublicId = order.PublicId,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CreatedByUserPublicId = order.CreatedByUserPublicId,
            CreatedByUserNameSnapshot = order.CreatedByUserNameSnapshot,
            SubtotalAmount = order.SubtotalAmount,
            GstRate = order.GstRate,
            GstAmount = order.GstAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Items = order.Items.Select(x => new OrderItemDto
            {
                ProductPublicId = x.ProductPublicId,
                ProductNameSnapshot = x.ProductNameSnapshot,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal,
                Notes = x.Notes
            }).ToList()
        };
    }
}