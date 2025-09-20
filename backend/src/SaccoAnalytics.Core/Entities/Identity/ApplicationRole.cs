using Microsoft.AspNetCore.Identity;

namespace SaccoAnalytics.Core.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsSystemRole { get; set; } = false;
}
