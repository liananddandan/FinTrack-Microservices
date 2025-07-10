using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public class ApplicationUser : IdentityUser<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long JwtVersion { get; set; } = 1;
    public bool IsFirstLogin { get; set; } = true;
}