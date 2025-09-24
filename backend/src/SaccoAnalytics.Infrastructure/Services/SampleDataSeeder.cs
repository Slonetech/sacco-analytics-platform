using Microsoft.EntityFrameworkCore;
using SaccoAnalytics.Core.Entities.Financial;
using SaccoAnalytics.Infrastructure.Data;

namespace SaccoAnalytics.Infrastructure.Services;

public class SampleDataSeeder
{
    private readonly ApplicationDbContext _context;

    public SampleDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedFinancialDataAsync(Guid tenantId)
    {
        // Check if data already exists
        if (await _context.Members.AnyAsync(m => m.TenantId == tenantId))
        {
            return; // Data already seeded
        }

        // Create and save members first
        var members = CreateSampleMembers(tenantId);
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Reload members with IDs
        var savedMembers = await _context.Members
            .Where(m => m.TenantId == tenantId)
            .ToListAsync();

        // Create accounts
        var accounts = CreateSampleAccounts(savedMembers);
        _context.Accounts.AddRange(accounts);
        await _context.SaveChangesAsync();

        // Reload accounts with IDs
        var savedAccounts = await _context.Accounts
            .Where(a => a.TenantId == tenantId)
            .ToListAsync();

        // Create transactions
        var transactions = CreateSampleTransactions(savedAccounts);
        _context.Transactions.AddRange(transactions);

        // Create loans
        var loans = CreateSampleLoans(savedMembers);
        _context.Loans.AddRange(loans);

        await _context.SaveChangesAsync();
    }

    private static List<Member> CreateSampleMembers(Guid tenantId)
    {
        var now = DateTime.UtcNow;
        
        return new List<Member>
        {
            new()
            {
                Id = Guid.NewGuid(),
                MemberNumber = "M001",
                FirstName = "John",
                LastName = "Kimani",
                Email = "john.kimani@email.com",
                PhoneNumber = "+254712345601",
                DateOfBirth = new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                JoinDate = now.AddMonths(-24),
                Address = "Nakuru, Kenya",
                Occupation = "Teacher",
                Employer = "Nakuru Primary School",
                MonthlyIncome = 45000,
                TenantId = tenantId,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                MemberNumber = "M002",
                FirstName = "Mary",
                LastName = "Wanjiku",
                Email = "mary.wanjiku@email.com",
                PhoneNumber = "+254712345602",
                DateOfBirth = new DateTime(1990, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                JoinDate = now.AddMonths(-18),
                Address = "Nakuru, Kenya",
                Occupation = "Nurse",
                Employer = "Nakuru General Hospital",
                MonthlyIncome = 55000,
                TenantId = tenantId,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                MemberNumber = "M003",
                FirstName = "Peter",
                LastName = "Mwangi",
                Email = "peter.mwangi@email.com",
                PhoneNumber = "+254712345603",
                DateOfBirth = new DateTime(1982, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                JoinDate = now.AddMonths(-36),
                Address = "Nakuru, Kenya",
                Occupation = "Engineer",
                Employer = "Kenya Power",
                MonthlyIncome = 75000,
                TenantId = tenantId,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                MemberNumber = "M004",
                FirstName = "Grace",
                LastName = "Nyokabi",
                Email = "grace.nyokabi@email.com",
                PhoneNumber = "+254712345604",
                DateOfBirth = new DateTime(1988, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                JoinDate = now.AddMonths(-12),
                Address = "Nakuru, Kenya",
                Occupation = "Accountant",
                Employer = "County Government",
                MonthlyIncome = 60000,
                TenantId = tenantId,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                MemberNumber = "M005",
                FirstName = "Samuel",
                LastName = "Kiprop",
                Email = "samuel.kiprop@email.com",
                PhoneNumber = "+254712345605",
                DateOfBirth = new DateTime(1986, 7, 18, 0, 0, 0, DateTimeKind.Utc),
                JoinDate = now.AddMonths(-6),
                Address = "Nakuru, Kenya",
                Occupation = "Business Owner",
                Employer = "Self Employed",
                MonthlyIncome = 80000,
                TenantId = tenantId,
                CreatedAt = now
            }
        };
    }

    private static List<Account> CreateSampleAccounts(List<Member> members)
    {
        var accounts = new List<Account>();
        var random = new Random(42);
        var now = DateTime.UtcNow;

        foreach (var member in members)
        {
            // Savings account for each member
            accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = $"SAV{member.MemberNumber}",
                AccountType = AccountType.Savings,
                Balance = random.Next(10000, 100000),
                InterestRate = 0.05m,
                OpenDate = member.JoinDate,
                MemberId = member.Id,
                TenantId = member.TenantId,
                CreatedAt = now
            });

            // Shares account for each member
            accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = $"SHR{member.MemberNumber}",
                AccountType = AccountType.Shares,
                Balance = random.Next(5000, 50000),
                InterestRate = 0.08m,
                OpenDate = member.JoinDate,
                MemberId = member.Id,
                TenantId = member.TenantId,
                CreatedAt = now
            });
        }

        return accounts;
    }

    private static List<Transaction> CreateSampleTransactions(List<Account> accounts)
    {
        var transactions = new List<Transaction>();
        var random = new Random(42);
        var transactionCounter = 1;
        var now = DateTime.UtcNow;

        foreach (var account in accounts)
        {
            var currentBalance = account.Balance;
            var transactionCount = random.Next(5, 15);
            
            for (int i = 0; i < transactionCount; i++)
            {
                var transactionDate = now.AddDays(-random.Next(1, 180));
                var isDeposit = random.NextDouble() > 0.4;
                var amount = random.Next(1000, 15000);

                if (!isDeposit && amount > currentBalance * 0.8m)
                {
                    amount = (int)(currentBalance * 0.5m);
                }

                if (!isDeposit)
                {
                    currentBalance -= amount;
                }
                else
                {
                    currentBalance += amount;
                }

                transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    TransactionReference = $"TXN{transactionCounter:D6}",
                    TransactionType = isDeposit ? TransactionType.Deposit : TransactionType.Withdrawal,
                    Amount = amount,
                    BalanceAfter = currentBalance,
                    TransactionDate = transactionDate,
                    Description = isDeposit ? "Cash Deposit" : "Cash Withdrawal",
                    Channel = random.NextDouble() > 0.5 ? "Branch" : "Mobile",
                    AccountId = account.Id,
                    TenantId = account.TenantId,
                    CreatedAt = now
                });

                transactionCounter++;
            }
        }

        return transactions;
    }

    private static List<Loan> CreateSampleLoans(List<Member> members)
    {
        var loans = new List<Loan>();
        var random = new Random(42);
        var loanTypes = Enum.GetValues<LoanType>();
        var loanCounter = 1;
        var now = DateTime.UtcNow;

        foreach (var member in members)
        {
            var loanCount = random.Next(1, 3);
            
            for (int i = 0; i < loanCount; i++)
            {
                var principal = random.Next(50000, 500000);
                var termMonths = new[] { 12, 24, 36, 48 }[random.Next(4)];
                var interestRate = 0.12m + (decimal)(random.NextDouble() * 0.08);
                var monthlyPayment = CalculateMonthlyPayment(principal, interestRate, termMonths);
                var applicationDate = now.AddDays(-random.Next(30, 365));
                
                loans.Add(new Loan
                {
                    Id = Guid.NewGuid(),
                    LoanNumber = $"LN{loanCounter:D3}",
                    LoanType = loanTypes[random.Next(loanTypes.Length)],
                    PrincipalAmount = principal,
                    InterestRate = interestRate,
                    TermInMonths = termMonths,
                    MonthlyPayment = monthlyPayment,
                    OutstandingBalance = principal * (decimal)(0.1 + random.NextDouble() * 0.9),
                    ApplicationDate = applicationDate,
                    ApprovalDate = applicationDate.AddDays(random.Next(1, 14)),
                    DisbursementDate = applicationDate.AddDays(random.Next(15, 30)),
                    MaturityDate = applicationDate.AddMonths(termMonths),
                    Status = random.NextDouble() > 0.3 ? LoanStatus.Active : LoanStatus.Completed,
                    Purpose = "Business development loan",
                    MemberId = member.Id,
                    TenantId = member.TenantId,
                    CreatedAt = now
                });
                
                loanCounter++;
            }
        }

        return loans;
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
    {
        if (annualRate == 0) return principal / months;
        
        var monthlyRate = annualRate / 12;
        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        return principal * monthlyRate * factor / (factor - 1);
    }
}
