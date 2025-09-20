using Microsoft.AspNetCore.Identity;

namespace SaccoAnalytics.Core.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Multi-tenancy - just the foreign key, no navigation property for now
    public Guid? TenantId { get; set; }
    
    // Note: Navigation properties will be configured in DbContext
}
