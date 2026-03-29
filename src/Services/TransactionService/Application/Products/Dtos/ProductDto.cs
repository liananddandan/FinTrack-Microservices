namespace TransactionService.Application.Products.Dtos;

public class ProductDto
{
    public Guid PublicId { get; set; }

    public Guid CategoryPublicId { get; set; }
    public string CategoryName { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsAvailable { get; set; }
}