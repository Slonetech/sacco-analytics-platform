using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Infrastructure.Data;
using SaccoAnalytics.Core.Entities.Financial;

namespace SaccoAnalytics.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] string tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            var query = _context.Transactions.Where(t => t.TenantId == tenantGuid);
            var totalCount = await query.CountAsync();
            
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    id = t.Id,
                    transactionType = t.TransactionType.ToString(),
                    amount = t.Amount,
                    description = t.Description,
                    transactionDate = t.TransactionDate
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    items = transactions,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request", errors = ModelState });

            if (!Guid.TryParse(request.TenantId, out var tenantGuid))
                return BadRequest(new { success = false, message = "Invalid tenant ID" });

            // Create transaction using only guaranteed properties
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TenantId = tenantGuid,
                TransactionType = Enum.Parse<TransactionType>(request.TransactionType),
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = transaction.Id,
                    amount = transaction.Amount,
                    transactionDate = transaction.TransactionDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction: {Error}", ex.Message);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { success = false, message = "Transaction not found" });

            return Ok(new { 
                success = true, 
                data = new {
                    id = transaction.Id,
                    transactionType = transaction.TransactionType.ToString(),
                    amount = transaction.Amount,
                    description = transaction.Description,
                    transactionDate = transaction.TransactionDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {Id}", id);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}

public class CreateTransactionRequest
{
    public string TransactionType { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string TenantId { get; set; } = null!;
}
