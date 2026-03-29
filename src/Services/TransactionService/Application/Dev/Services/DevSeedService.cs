using SharedKernel.Contracts.Dev;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Dev.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.ProductCategories.Abstractions;
using TransactionService.Application.Products.Abstractions;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Dev.Services;

public class DevSeedService(
    IUnitOfWork unitOfWork,
    IProductCategoryRepository productCategoryRepo,
    IProductRepository productRepo,
    IOrderRepository orderRepository) : IDevSeedService
{
    public async Task<DevTransactionSeedResult> SeedMenuAndOrdersAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.TenantPublicId, out var tenantPublicId))
        {
            throw new InvalidOperationException("TenantPublicId is invalid.");
        }

        if (!Guid.TryParse(request.MemberUserPublicId, out var memberUserPublicId))
        {
            throw new InvalidOperationException("MemberUserPublicId is invalid.");
        }

        if (!Guid.TryParse(request.AdminUserPublicId, out var adminUserPublicId))
        {
            throw new InvalidOperationException("AdminUserPublicId is invalid.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var categories = request.Template.ToLowerInvariant() switch
            {
                "coffee" => await SeedCoffeeMenuAsync(tenantPublicId, cancellationToken),
                "sushi" => await SeedSushiMenuAsync(tenantPublicId, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported template: {request.Template}")
            };

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var createdOrderIds = await SeedDemoOrdersAsync(
                tenantPublicId,
                request.TenantName,
                adminUserPublicId,
                memberUserPublicId,
                categories.SelectMany(x => x.Products).ToList(),
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return new DevTransactionSeedResult(
                CategoryCount: categories.Count,
                ProductCount: categories.Sum(x => x.Products.Count),
                OrderCount: createdOrderIds.Count,
                CreatedOrderPublicIds: createdOrderIds.Select(x => x.ToString()).ToList());
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<SeededCategory>> SeedCoffeeMenuAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        var coffee = await EnsureCategoryAsync(tenantPublicId, "Coffee", 1, cancellationToken);
        var dessert = await EnsureCategoryAsync(tenantPublicId, "Dessert", 2, cancellationToken);
        var coldDrinks = await EnsureCategoryAsync(tenantPublicId, "Cold Drinks", 3, cancellationToken);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        var coffeeProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, coffee, "Flat White", 5.50m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, coffee, "Latte", 5.80m, 2, cancellationToken),
            await EnsureProductAsync(tenantPublicId, coffee, "Cappuccino", 5.60m, 3, cancellationToken)
        };

        var dessertProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, dessert, "Cheesecake", 8.50m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, dessert, "Muffin", 6.00m, 2, cancellationToken)
        };

        var coldDrinkProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, coldDrinks, "Iced Americano", 6.00m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, coldDrinks, "Lemon Tea", 5.50m, 2, cancellationToken)
        };

        return
        [
            new SeededCategory(coffee, coffeeProducts),
            new SeededCategory(dessert, dessertProducts),
            new SeededCategory(coldDrinks, coldDrinkProducts)
        ];
    }

    private async Task<List<SeededCategory>> SeedSushiMenuAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        var rolls = await EnsureCategoryAsync(tenantPublicId, "Rolls", 1, cancellationToken);
        var nigiri = await EnsureCategoryAsync(tenantPublicId, "Nigiri", 2, cancellationToken);
        var drinks = await EnsureCategoryAsync(tenantPublicId, "Drinks", 3, cancellationToken);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        var rollProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, rolls, "Salmon Roll", 14.50m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, rolls, "Tuna Roll", 15.00m, 2, cancellationToken)
        };

        var nigiriProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, nigiri, "Salmon Nigiri", 7.50m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, nigiri, "Eel Nigiri", 8.00m, 2, cancellationToken)
        };

        var drinkProducts = new List<Product>
        {
            await EnsureProductAsync(tenantPublicId, drinks, "Miso Soup", 4.50m, 1, cancellationToken),
            await EnsureProductAsync(tenantPublicId, drinks, "Matcha", 5.50m, 2, cancellationToken)
        };

        return
        [
            new SeededCategory(rolls, rollProducts),
            new SeededCategory(nigiri, nigiriProducts),
            new SeededCategory(drinks, drinkProducts)
        ];
    }

    private async Task<ProductCategory> EnsureCategoryAsync(
        Guid tenantPublicId,
        string name,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        var existing = await productCategoryRepo.GetByNameAsync(
            tenantPublicId,
            name,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var category = new ProductCategory
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Name = name,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await productCategoryRepo.AddAsync(category, cancellationToken);
        return category;
    }

    private async Task<Product> EnsureProductAsync(
        Guid tenantPublicId,
        ProductCategory category,
        string name,
        decimal price,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        var existing = await productRepo.GetByNameAsync(
            tenantPublicId,
            name,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var product = new Product
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            CategoryId = category.Id,
            Name = name,
            Price = price,
            DisplayOrder = displayOrder,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        await productRepo.AddAsync(product, cancellationToken);
        return product;
    }

    private async Task<List<Guid>> SeedDemoOrdersAsync(
        Guid tenantPublicId,
        string tenantName,
        Guid adminUserPublicId,
        Guid memberUserPublicId,
        List<Product> products,
        CancellationToken cancellationToken)
    {
        var created = new List<Guid>();

        if (products.Count < 2)
        {
            return created;
        }

        var existing = await orderRepository.GetPagedAsync(
            tenantPublicId,
            null,
            null,
            null,
            null,
            1,
            10,
            cancellationToken);

        if (existing.TotalCount > 0)
        {
            return created;
        }

        var firstOrder = CreateSeedOrder(
            tenantPublicId,
            tenantName,
            memberUserPublicId,
            "Walk-in",
            "Cash",
            "ORD-DEMO-0001",
            products.Take(2).ToList());

        var secondOrder = CreateSeedOrder(
            tenantPublicId,
            tenantName,
            adminUserPublicId,
            "Emily",
            "Card",
            "ORD-DEMO-0002",
            products.Skip(1).Take(2).ToList());

        await orderRepository.AddAsync(firstOrder, cancellationToken);
        await orderRepository.AddAsync(secondOrder, cancellationToken);

        created.Add(firstOrder.PublicId);
        created.Add(secondOrder.PublicId);

        return created;
    }

    private static Order CreateSeedOrder(
        Guid tenantPublicId,
        string tenantName,
        Guid createdByUserPublicId,
        string? customerName,
        string paymentMethod,
        string orderNumber,
        List<Product> products)
    {
        var items = new List<OrderItem>();
        decimal grossAmount = 0m;
        var gstRate = 0.15m;

        foreach (var product in products)
        {
            var quantity = 1;
            var lineTotal = product.Price * quantity;

            items.Add(new OrderItem
            {
                ProductPublicId = product.PublicId,
                ProductNameSnapshot = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                LineTotal = lineTotal
            });

            grossAmount += lineTotal;
        }

        var discountAmount = 0m;
        var totalAmount = grossAmount - discountAmount;
        var subtotalAmount = Math.Round(totalAmount / (1 + gstRate), 2, MidpointRounding.AwayFromZero);
        var gstAmount = Math.Round(totalAmount - subtotalAmount, 2, MidpointRounding.AwayFromZero);

        return new Order
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            OrderNumber = orderNumber,
            CustomerName = customerName,
            CreatedByUserPublicId = createdByUserPublicId,
            CreatedByUserNameSnapshot = "Demo User",
            SubtotalAmount = subtotalAmount,
            GstRate = gstRate,
            GstAmount = gstAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Status = OrderStatuses.Completed,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = paymentMethod,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
            Items = items
        };
    }

    private sealed record SeededCategory(
        ProductCategory Category,
        List<Product> Products);
}