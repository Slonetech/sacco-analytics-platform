using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Core.Entities.Financial;
using SaccoAnalytics.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary([FromQuery] Guid? tenantId)
    {
        try
        {
            var effectiveTenantId = GetEffectiveTenantId(tenantId);
            if (effectiveTenantId == null)
            {
                return BadRequest("Tenant ID is required");
            }

            var summary = new DashboardSummaryDto();

            // Get member statistics
            summary.TotalMembers = await _context.Members
                .Where(m => m.TenantId == effectiveTenantId && m.IsActive)
                .CountAsync();

            summary.NewMembersThisMonth = await _context.Members
                .Where(m => m.TenantId == effectiveTenantId && 
                           m.IsActive && 
                           m.JoinDate >= DateTime.UtcNow.Date.AddDays(-30))
                .CountAsync();

            // Get account statistics
            var accounts = await _context.Accounts
                .Where(a => a.TenantId == effectiveTenantId && a.IsActive)
                .ToListAsync();

            summary.TotalSavings = accounts
                .Where(a => a.AccountType == AccountType.Savings)
                .Sum(a => a.Balance);

            summary.TotalShares = accounts
                .Where(a => a.AccountType == AccountType.Shares)
                .Sum(a => a.Balance);

            // Get loan statistics
            var activeLoans = await _context.Loans
                .Where(l => l.TenantId == effectiveTenantId && 
                           l.Status == LoanStatus.Active)
                .ToListAsync();

            summary.TotalLoansOutstanding = activeLoans.Sum(l => l.OutstandingBalance);
            summary.TotalLoansPrincipal = activeLoans.Sum(l => l.PrincipalAmount);

            // Get transaction statistics (last 30 days)
            var recentTransactions = await _context.Transactions
                .Where(t => t.TenantId == effectiveTenantId &&
                           t.TransactionDate >= DateTime.UtcNow.Date.AddDays(-30))
                .ToListAsync();

            summary.RecentDeposits = recentTransactions
                .Where(t => t.TransactionType == TransactionType.Deposit)
                .Sum(t => t.Amount);

            summary.RecentWithdrawals = recentTransactions
                .Where(t => t.TransactionType == TransactionType.Withdrawal)
                .Sum(t => t.Amount);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary");
            return StatusCode(500, new { message = "Error generating dashboard summary" });
        }
    }

    [HttpGet("members")]
    public async Task<ActionResult<IEnumerable<MemberReportDto>>> GetMembersReport([FromQuery] Guid? tenantId)
    {
        try
        {
            var effectiveTenantId = GetEffectiveTenantId(tenantId);
            if (effectiveTenantId == null)
            {
                return BadRequest("Tenant ID is required");
            }

            var members = await _context.Members
                .Where(m => m.TenantId == effectiveTenantId && m.IsActive)
                .Select(m => new MemberReportDto
                {
                    Id = m.Id,
                    MemberNumber = m.MemberNumber,
                    FullName = m.FullName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    JoinDate = m.JoinDate,
                    TotalSavings = m.Accounts
                        .Where(a => a.AccountType == AccountType.Savings && a.IsActive)
                        .Sum(a => a.Balance),
                    TotalShares = m.Accounts
                        .Where(a => a.AccountType == AccountType.Shares && a.IsActive)
                        .Sum(a => a.Balance),
                    ActiveLoans = m.Loans
                        .Where(l => l.Status == LoanStatus.Active)
                        .Sum(l => l.OutstandingBalance)
                })
                .OrderBy(m => m.MemberNumber)
                .ToListAsync();

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating members report");
            return StatusCode(500, new { message = "Error generating members report" });
        }
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<TransactionReportDto>>> GetTransactionsReport(
        [FromQuery] Guid? tenantId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] TransactionType? transactionType)
    {
        try
        {
            var effectiveTenantId = GetEffectiveTenantId(tenantId);
            if (effectiveTenantId == null)
            {
                return BadRequest("Tenant ID is required");
            }

            var query = _context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.Member)
                .Where(t => t.TenantId == effectiveTenantId);

            // Apply date filters
            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            if (transactionType.HasValue)
                query = query.Where(t => t.TransactionType == transactionType.Value);

            var transactions = await query
                .Select(t => new TransactionReportDto
                {
                    Id = t.Id,
                    TransactionReference = t.TransactionReference,
                    MemberName = t.Account.Member.FullName,
                    MemberNumber = t.Account.Member.MemberNumber,
                    AccountNumber = t.Account.AccountNumber,
                    AccountType = t.Account.AccountType.ToString(),
                    TransactionType = t.TransactionType.ToString(),
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Channel = t.Channel
                })
                .OrderByDescending(t => t.TransactionDate)
                .Take(1000) // Limit for performance
                .ToListAsync();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transactions report");
            return StatusCode(500, new { message = "Error generating transactions report" });
        }
    }

    [HttpGet("loans")]
    public async Task<ActionResult<IEnumerable<LoanReportDto>>> GetLoansReport([FromQuery] Guid? tenantId)
    {
        try
        {
            var effectiveTenantId = GetEffectiveTenantId(tenantId);
            if (effectiveTenantId == null)
            {
                return BadRequest("Tenant ID is required");
            }

            var loans = await _context.Loans
                .Include(l => l.Member)
                .Where(l => l.TenantId == effectiveTenantId)
                .Select(l => new LoanReportDto
                {
                    Id = l.Id,
                    LoanNumber = l.LoanNumber,
                    MemberName = l.Member.FullName,
                    MemberNumber = l.Member.MemberNumber,
                    LoanType = l.LoanType.ToString(),
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TermInMonths = l.TermInMonths,
                    MonthlyPayment = l.MonthlyPayment,
                    OutstandingBalance = l.OutstandingBalance,
                    Status = l.Status.ToString(),
                    ApplicationDate = l.ApplicationDate,
                    DisbursementDate = l.DisbursementDate,
                    MaturityDate = l.MaturityDate
                })
                .OrderByDescending(l => l.ApplicationDate)
                .ToListAsync();

            return Ok(loans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating loans report");
            return StatusCode(500, new { message = "Error generating loans report" });
        }
    }

    private Guid? GetEffectiveTenantId(Guid? requestedTenantId)
    {
        // System admins can query any tenant
        if (User.IsInRole("SystemAdmin"))
        {
            return requestedTenantId;
        }

        // Other users can only access their own tenant data
        var tenantIdClaim = User.FindFirst("tenantId");
        return tenantIdClaim != null ? Guid.Parse(tenantIdClaim.Value) : null;
    }
}

// DTOs
public class DashboardSummaryDto
{
    public int TotalMembers { get; set; }
    public int NewMembersThisMonth { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalShares { get; set; }
    public decimal TotalLoansOutstanding { get; set; }
    public decimal TotalLoansPrincipal { get; set; }
    public decimal RecentDeposits { get; set; }
    public decimal RecentWithdrawals { get; set; }
}

public class MemberReportDto
{
    public Guid Id { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime JoinDate { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalShares { get; set; }
    public decimal ActiveLoans { get; set; }
}

public class TransactionReportDto
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public string? Channel { get; set; }
}

public class LoanReportDto
{
    public Guid Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermInMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime? MaturityDate { get; set; }
}
