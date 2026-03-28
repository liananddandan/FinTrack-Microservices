namespace TransactionService.Application.DTOs;

public class ProductCategoryDto
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}