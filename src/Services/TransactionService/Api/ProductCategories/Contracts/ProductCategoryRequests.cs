namespace TransactionService.Api.ProductCategories.Contracts;

public record CreateProductCategoryRequest(
    string Name,
    int DisplayOrder
);

public record UpdateProductCategoryRequest(
    string Name,
    int DisplayOrder,
    bool IsActive
);