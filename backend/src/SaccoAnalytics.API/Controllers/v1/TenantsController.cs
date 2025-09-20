using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Core.Entities.Tenants;
using SaccoAnalytics.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ApplicationDbContext context, ILogger<TenantsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenants()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsActive)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                ContactEmail = t.ContactEmail,
                ContactPhone = t.ContactPhone,
                IsActive = t.IsActive,
                SubscriptionStart = t.SubscriptionStart,
                SubscriptionEnd = t.SubscriptionEnd,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(tenants);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SystemAdmin,TenantAdmin")]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        
        if (tenant == null || !tenant.IsActive)
        {
            return NotFound();
        }

        var tenantDto = new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            Description = tenant.Description,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            IsActive = tenant.IsActive,
            SubscriptionStart = tenant.SubscriptionStart,
            SubscriptionEnd = tenant.SubscriptionEnd,
            CreatedAt = tenant.CreatedAt
        };

        return Ok(tenantDto);
    }

    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        try
        {
            // Check if code already exists
            if (await _context.Tenants.AnyAsync(t => t.Code == request.Code))
            {
                return BadRequest(new { message = "Tenant code already exists" });
            }

            // Check if email already exists
            if (await _context.Tenants.AnyAsync(t => t.ContactEmail == request.ContactEmail))
            {
                return BadRequest(new { message = "Contact email already exists" });
            }

            var tenant = new Tenant
            {
                Name = request.Name,
                Code = request.Code.ToUpper(),
                Description = request.Description,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                IsActive = true,
                SubscriptionStart = request.SubscriptionStart ?? DateTime.UtcNow,
                SubscriptionEnd = request.SubscriptionEnd,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant {TenantName} created successfully with ID {TenantId}", 
                tenant.Name, tenant.Id);

            var tenantDto = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Code = tenant.Code,
                Description = tenant.Description,
                ContactEmail = tenant.ContactEmail,
                ContactPhone = tenant.ContactPhone,
                IsActive = tenant.IsActive,
                SubscriptionStart = tenant.SubscriptionStart,
                SubscriptionEnd = tenant.SubscriptionEnd,
                CreatedAt = tenant.CreatedAt
            };

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenantDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return StatusCode(500, new { message = "An error occurred while creating the tenant" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            // Check if new email conflicts with other tenants
            if (request.ContactEmail != tenant.ContactEmail && 
                await _context.Tenants.AnyAsync(t => t.ContactEmail == request.ContactEmail && t.Id != id))
            {
                return BadRequest(new { message = "Contact email already exists" });
            }

            tenant.Name = request.Name;
            tenant.Description = request.Description;
            tenant.ContactEmail = request.ContactEmail;
            tenant.ContactPhone = request.ContactPhone;
            tenant.SubscriptionEnd = request.SubscriptionEnd;
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.UpdatedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the tenant" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            // Soft delete
            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.UpdatedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant {TenantId} soft deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the tenant" });
        }
    }
}

// DTOs
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
}

public class UpdateTenantRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string? ContactPhone { get; set; }

    public DateTime? SubscriptionEnd { get; set; }
}
