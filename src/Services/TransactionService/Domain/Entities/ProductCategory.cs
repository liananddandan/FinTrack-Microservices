namespace TransactionService.Domain.Entities;

public class ProductCategory
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }

    public required string Name { get; set; }
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}