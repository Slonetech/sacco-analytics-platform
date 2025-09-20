using SaccoAnalytics.Core.Entities.Common;

namespace SaccoAnalytics.Core.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public DateTime? RevokedAt { get; set; }
    
    public bool IsActive => RevokedAt == null && !IsExpired;
    
    public string? ReplacedByToken { get; set; }
    
    public string? ReasonRevoked { get; set; }
    
    // Foreign key - navigation property configured in DbContext
    public string UserId { get; set; } = string.Empty;
}
