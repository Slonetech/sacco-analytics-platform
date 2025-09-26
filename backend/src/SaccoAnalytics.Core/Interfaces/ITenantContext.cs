namespace SaccoAnalytics.Core.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantCode { get; }
    bool HasTenant { get; }
    void SetTenant(Guid tenantId, string tenantCode);
}