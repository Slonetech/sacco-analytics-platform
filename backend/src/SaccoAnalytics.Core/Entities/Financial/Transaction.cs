using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Financial;

public class Transaction : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    public TransactionType TransactionType { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    public decimal BalanceAfter { get; set; }
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Channel { get; set; } // Mobile, Branch, Online, etc.
    
    // Foreign keys
    public Guid AccountId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual Account Account { get; set; } = null!;
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Interest = 4,
    Fees = 5,
    LoanDisbursement = 6,
    LoanRepayment = 7
}
