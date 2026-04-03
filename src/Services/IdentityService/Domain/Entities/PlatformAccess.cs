namespace IdentityService.Domain.Entities;

public class PlatformAccess
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string UserPublicId { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}