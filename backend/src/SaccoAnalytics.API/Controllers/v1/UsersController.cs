using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Core.Entities.Identity;
using SaccoAnalytics.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,TenantAdmin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] Guid? tenantId)
    {
        var query = _context.Users.AsQueryable();

        if (User.IsInRole("TenantAdmin"))
        {
            // Tenant admins can only see users from their own tenant
            var currentUserTenantId = GetCurrentUserTenantId();
            query = query.Where(u => u.TenantId == currentUserTenantId);
        }
        else if (tenantId.HasValue)
        {
            // System admins can filter by specific tenant
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        var users = await query
            .Where(u => u.IsActive)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                TenantId = u.TenantId,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("assign-to-tenant")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> AssignUserToTenant([FromBody] AssignUserToTenantRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var tenant = await _context.Tenants.FindAsync(request.TenantId);
            if (tenant == null || !tenant.IsActive)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            user.TenantId = request.TenantId;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to assign user to tenant", errors = result.Errors });
            }

            _logger.LogInformation("User {UserId} assigned to tenant {TenantId}", request.UserId, request.TenantId);
            return Ok(new { message = "User successfully assigned to tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user to tenant");
            return StatusCode(500, new { message = "An error occurred while assigning user to tenant" });
        }
    }

    [HttpPost("{userId}/roles")]
    [Authorize(Roles = "SystemAdmin,TenantAdmin")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Tenant admins can only manage users in their own tenant
            if (User.IsInRole("TenantAdmin"))
            {
                var currentUserTenantId = GetCurrentUserTenantId();
                if (user.TenantId != currentUserTenantId)
                {
                    return Forbid();
                }
                
                // Tenant admins cannot assign SystemAdmin role
                if (request.RoleName == "SystemAdmin")
                {
                    return Forbid();
                }
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
            }

            return Ok(new { message = $"Role {request.RoleName} assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return StatusCode(500, new { message = "An error occurred while assigning role" });
        }
    }

    private Guid? GetCurrentUserTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenantId");
        return tenantIdClaim != null ? Guid.Parse(tenantIdClaim.Value) : null;
    }
}

// DTOs
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AssignUserToTenantRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid TenantId { get; set; }
}

public class AssignRoleRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}
