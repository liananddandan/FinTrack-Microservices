namespace TransactionService.Api.Products.Contracts;

public record CreateProductRequest(
    Guid CategoryPublicId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int? DisplayOrder
);

public record UpdateProductRequest(
    Guid CategoryPublicId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int? DisplayOrder,
    bool IsAvailable
);