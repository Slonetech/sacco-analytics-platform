using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Infrastructure.Data;
using SaccoAnalytics.Core.Entities.Financial;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MembersController> _logger;

    public MembersController(ApplicationDbContext context, ILogger<MembersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers([FromQuery] string tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try 
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            var query = _context.Members.Where(m => m.TenantId == tenantGuid);
            var totalCount = await query.CountAsync();
            
            var members = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    id = m.Id,
                    memberNumber = m.MemberNumber,
                    firstName = m.FirstName,
                    lastName = m.LastName,
                    email = m.Email,
                    phoneNumber = m.PhoneNumber,
                    dateJoined = m.CreatedAt,
                    isActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new { 
                success = true, 
                data = new { 
                    items = members,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request)
    {
        try 
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });

            if (!Guid.TryParse(request.TenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            // Create member using only guaranteed properties
            var member = new Member
            {
                Id = Guid.NewGuid(),
                TenantId = tenantGuid,
                MemberNumber = request.MemberNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = DateTime.Parse(request.DateOfBirth),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                data = new { 
                    id = member.Id,
                    memberNumber = member.MemberNumber,
                    firstName = member.FirstName,
                    lastName = member.LastName,
                    email = member.Email
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member: {Error}", ex.Message);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMember(Guid id)
    {
        try 
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { success = false, message = "Member not found" });

            return Ok(new { 
                success = true, 
                data = new {
                    id = member.Id,
                    memberNumber = member.MemberNumber,
                    firstName = member.FirstName,
                    lastName = member.LastName,
                    email = member.Email,
                    phoneNumber = member.PhoneNumber,
                    dateOfBirth = member.DateOfBirth,
                    isActive = member.IsActive,
                    dateJoined = member.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member {Id}", id);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberRequest request)
    {
        try 
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
                return NotFound(new { success = false, message = "Member not found" });

            if (!string.IsNullOrEmpty(request.FirstName)) member.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) member.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Email)) member.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) member.PhoneNumber = request.PhoneNumber;
            
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Member updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member {Id}", id);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMembers([FromQuery] string tenantId, [FromQuery] string query)
    {
        try 
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            if (string.IsNullOrWhiteSpace(query))
                return Ok(new { success = true, data = new List<object>() });

            var members = await _context.Members
                .Where(m => m.TenantId == tenantGuid &&
                           (m.FirstName.Contains(query) || m.LastName.Contains(query) || 
                            m.MemberNumber.Contains(query) || m.Email.Contains(query)))
                .Take(20)
                .Select(m => new
                {
                    id = m.Id,
                    memberNumber = m.MemberNumber,
                    firstName = m.FirstName,
                    lastName = m.LastName,
                    email = m.Email
                })
                .ToListAsync();

            return Ok(new { success = true, data = members });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching members");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}

public class CreateMemberRequest
{
    public string MemberNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string DateOfBirth { get; set; } = null!;
    public string TenantId { get; set; } = null!;
}

public class UpdateMemberRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
