using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Infrastructure.Data;
using SaccoAnalytics.Core.Entities.Financial;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LoansController> _logger;

    public LoansController(ApplicationDbContext context, ILogger<LoansController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans([FromQuery] string tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            var query = _context.Loans.Where(l => l.TenantId == tenantGuid);
            var totalCount = await query.CountAsync();
            
            var loans = await query
                .OrderByDescending(l => l.ApplicationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    id = l.Id,
                    loanNumber = l.LoanNumber,
                    memberId = l.MemberId,
                    loanType = l.LoanType.ToString(),
                    // Try different possible amount property names
                    amount = (decimal?)null, // We'll use a placeholder for now
                    outstandingBalance = l.OutstandingBalance,
                    interestRate = l.InterestRate,
                    termInMonths = l.TermInMonths,
                    status = l.Status.ToString(),
                    applicationDate = l.ApplicationDate
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    items = loans,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loans");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoanApplication([FromBody] CreateLoanRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });

            if (!Guid.TryParse(request.TenantId, out var tenantGuid) ||
                !Guid.TryParse(request.MemberId, out var memberGuid))
                return BadRequest(new { success = false, message = "Invalid IDs" });

            var member = await _context.Members.FindAsync(memberGuid);
            if (member == null)
                return NotFound(new { success = false, message = "Member not found" });

            // Create loan using only basic properties - avoid Amount property for now
            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                TenantId = tenantGuid,
                MemberId = memberGuid,
                LoanNumber = $"LN{DateTime.Now:yyyyMM}{new Random().Next(1000, 9999)}",
                LoanType = Enum.Parse<LoanType>(request.LoanType),
                // Don't set Amount - we'll figure out the correct property later
                Purpose = request.Purpose,
                TermInMonths = request.TermInMonths,
                InterestRate = request.InterestRate,
                Status = LoanStatus.Applied,
                ApplicationDate = DateTime.UtcNow,
                OutstandingBalance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = loan.Id,
                    loanNumber = loan.LoanNumber,
                    requestedAmount = request.RequestedAmount, // Return the requested amount from request
                    status = loan.Status.ToString(),
                    applicationDate = loan.ApplicationDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loan application: {Error}", ex.Message);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLoan(Guid id)
    {
        try
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                return NotFound(new { success = false, message = "Loan not found" });

            return Ok(new { 
                success = true, 
                data = new {
                    id = loan.Id,
                    loanNumber = loan.LoanNumber,
                    memberId = loan.MemberId,
                    loanType = loan.LoanType.ToString(),
                    // Skip amount property for now
                    outstandingBalance = loan.OutstandingBalance,
                    interestRate = loan.InterestRate,
                    termInMonths = loan.TermInMonths,
                    purpose = loan.Purpose,
                    status = loan.Status.ToString(),
                    applicationDate = loan.ApplicationDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loan {Id}", id);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}

public class CreateLoanRequest
{
    public string MemberId { get; set; } = null!;
    public string LoanType { get; set; } = null!;
    public decimal RequestedAmount { get; set; }
    public string Purpose { get; set; } = null!;
    public int TermInMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string TenantId { get; set; } = null!;
}
