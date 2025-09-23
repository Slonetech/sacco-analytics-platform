using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Financial;

public class LoanPayment : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string PaymentReference { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    public decimal PrincipalAmount { get; set; }
    
    public decimal InterestAmount { get; set; }
    
    public decimal PenaltyAmount { get; set; }
    
    [Required]
    public DateTime PaymentDate { get; set; }
    
    public DateTime DueDate { get; set; }
    
    [StringLength(200)]
    public string? Notes { get; set; }
    
    // Foreign keys
    public Guid LoanId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual Loan Loan { get; set; } = null!;
}
