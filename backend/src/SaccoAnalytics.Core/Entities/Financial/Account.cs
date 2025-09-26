using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Financial;

public class Account : BaseEntity, ITenantEntity
{
    [Required]
    [StringLength(20)]
    public string AccountNumber { get; set; } = string.Empty;
    
    [Required]
    public AccountType AccountType { get; set; }
    
    [Required]
    public decimal Balance { get; set; }
    
    [Required]
    public decimal InterestRate { get; set; }
    
    public DateTime OpenDate { get; set; }
    
    public DateTime? CloseDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Foreign keys
    public Guid MemberId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual Member Member { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    Savings = 1,
    Current = 2,
    FixedDeposit = 3,
    Shares = 4
}
