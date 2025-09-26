using SaccoAnalytics.Core.Interfaces;

namespace SaccoAnalytics.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    private string? _tenantCode;

    public Guid? TenantId => _tenantId;
    public string? TenantCode => _tenantCode;
    public bool HasTenant => _tenantId.HasValue;

    public void SetTenant(Guid tenantId, string tenantCode)
    {
        _tenantId = tenantId;
        _tenantCode = tenantCode;
    }
}