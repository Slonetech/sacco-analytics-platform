using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Core.Entities.Identity;
using SaccoAnalytics.Core.Entities.Tenants;
using SaccoAnalytics.Core.Entities.Financial;
using SaccoAnalytics.Core.Entities.Common;
using SaccoAnalytics.Core.Interfaces;
using System.Linq.Expressions;

namespace SaccoAnalytics.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly ITenantContext _tenantContext;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Identity & Tenancy
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Financial
    public DbSet<Member> Members { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanPayment> LoanPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureTenantEntity(builder);
        ConfigureUserEntity(builder);
        ConfigureRefreshTokenEntity(builder);
        ConfigureFinancialEntities(builder);
        
        // Apply tenant filtering for all tenant entities
        ApplyTenantFilters(builder);
    }
    
    // Automatic tenant filtering
    private void ApplyTenantFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(GetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);
                
                var filter = method?.Invoke(null, new object[] { _tenantContext });
                
                if (filter != null)
                {
                    entityType.SetQueryFilter((LambdaExpression)filter);
                }
            }
        }
    }
    
    private static LambdaExpression GetTenantFilter<TEntity>(ITenantContext tenantContext) 
        where TEntity : class, ITenantEntity
    {
        Expression<Func<TEntity, bool>> filter = e => !tenantContext.HasTenant || e.TenantId == tenantContext.TenantId;
        return filter;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set TenantId for new tenant entities
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (_tenantContext.HasTenant && entry.Entity.TenantId == Guid.Empty)
                        entry.Entity.TenantId = _tenantContext.TenantId!.Value;
                    break;
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureTenantEntity(ModelBuilder builder)
    {
        builder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });
    }

    private static void ConfigureUserEntity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRefreshTokenEntity(ModelBuilder builder)
    {
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureFinancialEntities(ModelBuilder builder)
    {
        // Member configuration
        builder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => new { e.MemberNumber, e.TenantId }).IsUnique();
            entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();

            entity.HasMany(m => m.Accounts)
                  .WithOne(a => a.Member)
                  .HasForeignKey(a => a.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.Loans)
                  .WithOne(l => l.Member)
                  .HasForeignKey(l => l.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Account configuration
        builder.Entity<Account>(entity =>
        {
            entity.HasIndex(e => new { e.AccountNumber, e.TenantId }).IsUnique();

            entity.HasMany(a => a.Transactions)
                  .WithOne(t => t.Account)
                  .HasForeignKey(t => t.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 4);
        });

        // Transaction configuration
        builder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionReference, e.TenantId }).IsUnique();
            entity.HasIndex(e => e.TransactionDate);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
        });

        // Loan configuration
        builder.Entity<Loan>(entity =>
        {
            entity.HasIndex(e => new { e.LoanNumber, e.TenantId }).IsUnique();

            entity.HasMany(l => l.Payments)
                  .WithOne(p => p.Loan)
                  .HasForeignKey(p => p.LoanId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 4);
            entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
            entity.Property(e => e.OutstandingBalance).HasPrecision(18, 2);
        });

        // LoanPayment configuration
        builder.Entity<LoanPayment>(entity =>
        {
            entity.HasIndex(e => new { e.PaymentReference, e.TenantId }).IsUnique();
            entity.HasIndex(e => e.PaymentDate);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestAmount).HasPrecision(18, 2);
            entity.Property(e => e.PenaltyAmount).HasPrecision(18, 2);
        });
    }
}