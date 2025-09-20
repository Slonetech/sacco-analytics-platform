using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Tenants;

public class Tenant : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? SubscriptionStart { get; set; }
    
    public DateTime? SubscriptionEnd { get; set; }
    
    // Note: Navigation property will be configured in DbContext
    // We don't define it here to avoid circular references
}
