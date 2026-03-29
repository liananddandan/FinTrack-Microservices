namespace TransactionService.Domain.Entities;

public class Product
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }

    public long CategoryId { get; set; }
    public ProductCategory Category { get; set; } = default!;

    public required string Name { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public int DisplayOrder { get; set; }
}