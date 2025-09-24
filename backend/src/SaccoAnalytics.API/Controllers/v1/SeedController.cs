using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Infrastructure.Data;
using SaccoAnalytics.Infrastructure.Services;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SystemAdmin")]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(ApplicationDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("financial-data/{tenantId}")]
    public async Task<IActionResult> SeedFinancialData(Guid tenantId)
    {
        try
        {
            // Verify tenant exists
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            // Check if data already exists
            var existingMembers = await _context.Members.CountAsync(m => m.TenantId == tenantId);
            if (existingMembers > 0)
            {
                return BadRequest(new { 
                    message = $"Financial data already exists for this tenant. Found {existingMembers} members." 
                });
            }

            var seeder = new SampleDataSeeder(_context);
            await seeder.SeedFinancialDataAsync(tenantId);

            // Get counts for confirmation
            var memberCount = await _context.Members.CountAsync(m => m.TenantId == tenantId);
            var accountCount = await _context.Accounts.CountAsync(a => a.TenantId == tenantId);
            var transactionCount = await _context.Transactions.CountAsync(t => t.TenantId == tenantId);
            var loanCount = await _context.Loans.CountAsync(l => l.TenantId == tenantId);

            _logger.LogInformation("Seeded financial data for tenant {TenantId}: {Members} members, {Accounts} accounts, {Transactions} transactions, {Loans} loans", 
                tenantId, memberCount, accountCount, transactionCount, loanCount);

            return Ok(new
            {
                message = "Sample financial data created successfully",
                tenantId = tenantId,
                tenantName = tenant.Name,
                data = new
                {
                    members = memberCount,
                    accounts = accountCount,
                    transactions = transactionCount,
                    loans = loanCount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding financial data for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred while seeding data" });
        }
    }

    [HttpDelete("financial-data/{tenantId}")]
    public async Task<IActionResult> ClearFinancialData(Guid tenantId)
    {
        try
        {
            // Remove in correct order due to foreign key constraints
            var transactions = await _context.Transactions.Where(t => t.TenantId == tenantId).ToListAsync();
            _context.Transactions.RemoveRange(transactions);

            var loanPayments = await _context.LoanPayments.Where(lp => lp.TenantId == tenantId).ToListAsync();
            _context.LoanPayments.RemoveRange(loanPayments);

            var loans = await _context.Loans.Where(l => l.TenantId == tenantId).ToListAsync();
            _context.Loans.RemoveRange(loans);

            var accounts = await _context.Accounts.Where(a => a.TenantId == tenantId).ToListAsync();
            _context.Accounts.RemoveRange(accounts);

            var members = await _context.Members.Where(m => m.TenantId == tenantId).ToListAsync();
            _context.Members.RemoveRange(members);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Financial data cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing financial data for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "An error occurred while clearing data" });
        }
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetAvailableTenants()
    {
        try
        {
            var tenants = await _context.Tenants
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    code = t.Code,
                    memberCount = _context.Members.Count(m => m.TenantId == t.Id)
                })
                .ToListAsync();

            return Ok(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tenants");
            return StatusCode(500, new { message = "An error occurred while getting tenants" });
        }
    }
}
