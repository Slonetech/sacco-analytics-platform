using SaccoAnalytics.Core.Interfaces;
using System.Security.Claims;

namespace SaccoAnalytics.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = await ResolveTenantIdAsync(context);
        
        if (tenantId != null)
        {
            // TODO: Load tenant code from database if needed
            tenantContext.SetTenant(tenantId.Value, "DEMO_SACCO");
        }
        
        await _next(context);
    }
    
    private Task<Guid?> ResolveTenantIdAsync(HttpContext context)
    {
        // Strategy 1: From JWT claims (most secure for your setup)
        var tenantClaim = context.User?.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantClaim, out var claimTenantId))
            return Task.FromResult<Guid?>(claimTenantId);
        
        // Strategy 2: From headers (for development/testing)
        var headerValue = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (Guid.TryParse(headerValue, out var headerTenantId))
            return Task.FromResult<Guid?>(headerTenantId);
        
        // Strategy 3: From subdomain (future consideration)
        // var host = context.Request.Host.Host;
        // Parse subdomain logic here...
        
        return Task.FromResult<Guid?>(null);
    }
}