using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Financial;

public class Loan : BaseEntity, ITenantEntity
{
    [Required]
    [StringLength(20)]
    public string LoanNumber { get; set; } = string.Empty;
    
    [Required]
    public LoanType LoanType { get; set; }
    
    [Required]
    public decimal PrincipalAmount { get; set; }
    
    [Required]
    public decimal InterestRate { get; set; }
    
    [Required]
    public int TermInMonths { get; set; }
    
    public decimal MonthlyPayment { get; set; }
    
    public decimal OutstandingBalance { get; set; }
    
    public DateTime ApplicationDate { get; set; }
    
    public DateTime? ApprovalDate { get; set; }
    
    public DateTime? DisbursementDate { get; set; }
    
    public DateTime? MaturityDate { get; set; }
    
    [Required]
    public LoanStatus Status { get; set; }
    
    [StringLength(500)]
    public string? Purpose { get; set; }
    
    // Foreign keys
    public Guid MemberId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual Member Member { get; set; } = null!;
    public virtual ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
}

public enum LoanType
{
    Personal = 1,
    Business = 2,
    Emergency = 3,
    Development = 4,
    Education = 5,
    Agriculture = 6
}

public enum LoanStatus
{
    Applied = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Disbursed = 5,
    Active = 6,
    Completed = 7,
    Defaulted = 8
}
