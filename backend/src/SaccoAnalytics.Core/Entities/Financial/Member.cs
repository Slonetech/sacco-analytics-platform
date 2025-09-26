using SaccoAnalytics.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SaccoAnalytics.Core.Entities.Financial;

public class Member : BaseEntity, ITenantEntity
{
    [Required]
    [StringLength(20)]
    public string MemberNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    public DateTime JoinDate { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Occupation { get; set; }
    
    [StringLength(100)]
    public string? Employer { get; set; }
    
    public decimal MonthlyIncome { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Multi-tenancy
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
