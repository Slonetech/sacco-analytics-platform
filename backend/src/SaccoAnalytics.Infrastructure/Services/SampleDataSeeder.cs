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

        var members = CreateSampleMembers(tenantId);
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        var accounts = CreateSampleAccounts(members);
        _context.Accounts.AddRange(accounts);
        await _context.SaveChangesAsync();

        var transactions = CreateSampleTransactions(accounts);
        _context.Transactions.AddRange(transactions);

        var loans = CreateSampleLoans(members);
        _context.Loans.AddRange(loans);

        await _context.SaveChangesAsync();
    }

    private static List<Member> CreateSampleMembers(Guid tenantId)
    {
        return new List<Member>
        {
            new Member
            {
                MemberNumber = "M001",
                FirstName = "John",
                LastName = "Kimani",
                Email = "john.kimani@email.com",
                PhoneNumber = "+254712345601",
                DateOfBirth = new DateTime(1985, 5, 15),
                JoinDate = DateTime.UtcNow.AddMonths(-24),
                Address = "Nakuru, Kenya",
                Occupation = "Teacher",
                Employer = "Nakuru Primary School",
                MonthlyIncome = 45000,
                TenantId = tenantId
            },
            new Member
            {
                MemberNumber = "M002",
                FirstName = "Mary",
                LastName = "Wanjiku",
                Email = "mary.wanjiku@email.com",
                PhoneNumber = "+254712345602",
                DateOfBirth = new DateTime(1990, 8, 22),
                JoinDate = DateTime.UtcNow.AddMonths(-18),
                Address = "Nakuru, Kenya",
                Occupation = "Nurse",
                Employer = "Nakuru General Hospital",
                MonthlyIncome = 55000,
                TenantId = tenantId
            },
            new Member
            {
                MemberNumber = "M003",
                FirstName = "Peter",
                LastName = "Mwangi",
                Email = "peter.mwangi@email.com",
                PhoneNumber = "+254712345603",
                DateOfBirth = new DateTime(1982, 12, 10),
                JoinDate = DateTime.UtcNow.AddMonths(-36),
                Address = "Nakuru, Kenya",
                Occupation = "Engineer",
                Employer = "Kenya Power",
                MonthlyIncome = 75000,
                TenantId = tenantId
            },
            new Member
            {
                MemberNumber = "M004",
                FirstName = "Grace",
                LastName = "Nyokabi",
                Email = "grace.nyokabi@email.com",
                PhoneNumber = "+254712345604",
                DateOfBirth = new DateTime(1988, 3, 5),
                JoinDate = DateTime.UtcNow.AddMonths(-12),
                Address = "Nakuru, Kenya",
                Occupation = "Accountant",
                Employer = "County Government",
                MonthlyIncome = 60000,
                TenantId = tenantId
            },
            new Member
            {
                MemberNumber = "M005",
                FirstName = "Samuel",
                LastName = "Kiprop",
                Email = "samuel.kiprop@email.com",
                PhoneNumber = "+254712345605",
                DateOfBirth = new DateTime(1986, 7, 18),
                JoinDate = DateTime.UtcNow.AddMonths(-6),
                Address = "Nakuru, Kenya",
                Occupation = "Business Owner",
                Employer = "Self Employed",
                MonthlyIncome = 80000,
                TenantId = tenantId
            }
        };
    }

    private static List<Account> CreateSampleAccounts(List<Member> members)
    {
        var accounts = new List<Account>();
        var random = new Random(42); // Fixed seed for consistent data

        foreach (var member in members)
        {
            // Savings account for each member
            accounts.Add(new Account
            {
                AccountNumber = $"SAV{member.MemberNumber}",
                AccountType = AccountType.Savings,
                Balance = random.Next(10000, 100000),
                InterestRate = 0.05m, // 5% interest
                OpenDate = member.JoinDate,
                MemberId = member.Id,
                TenantId = member.TenantId
            });

            // Shares account for each member
            accounts.Add(new Account
            {
                AccountNumber = $"SHR{member.MemberNumber}",
                AccountType = AccountType.Shares,
                Balance = random.Next(5000, 50000),
                InterestRate = 0.08m, // 8% dividend
                OpenDate = member.JoinDate,
                MemberId = member.Id,
                TenantId = member.TenantId
            });
        }

        return accounts;
    }

    private static List<Transaction> CreateSampleTransactions(List<Account> accounts)
    {
        var transactions = new List<Transaction>();
        var random = new Random(42);
        var transactionCounter = 1;

        foreach (var account in accounts)
        {
            var currentBalance = account.Balance;
            
            // Create 10-20 transactions per account
            var transactionCount = random.Next(10, 21);
            
            for (int i = 0; i < transactionCount; i++)
            {
                var transactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 365));
                var isDeposit = random.NextDouble() > 0.3; // 70% deposits, 30% withdrawals
                var amount = random.Next(1000, 10000);

                if (!isDeposit)
                {
                    amount = Math.Min(amount, (int)(currentBalance * 0.8m)); // Don't overdraw
                    currentBalance -= amount;
                }
                else
                {
                    currentBalance += amount;
                }

                transactions.Add(new Transaction
                {
                    TransactionReference = $"TXN{transactionCounter:D6}",
                    TransactionType = isDeposit ? TransactionType.Deposit : TransactionType.Withdrawal,
                    Amount = amount,
                    BalanceAfter = currentBalance,
                    TransactionDate = transactionDate,
                    Description = isDeposit ? "Cash Deposit" : "Cash Withdrawal",
                    Channel = random.NextDouble() > 0.5 ? "Branch" : "Mobile",
                    AccountId = account.Id,
                    TenantId = account.TenantId
                });

                transactionCounter++;
            }
        }

        return transactions.OrderBy(t => t.TransactionDate).ToList();
    }

    private static List<Loan> CreateSampleLoans(List<Member> members)
    {
        var loans = new List<Loan>();
        var random = new Random(42);
        var loanTypes = Enum.GetValues<LoanType>();
        var loanCounter = 1;

        // Create 1-3 loans per member
        foreach (var member in members)
        {
            var loanCount = random.Next(1, 4);
            
            for (int i = 0; i < loanCount; i++)
            {
                var principal = random.Next(50000, 500000);
                var termMonths = new[] { 12, 24, 36, 48 }[random.Next(4)];
                var interestRate = 0.12m + (decimal)(random.NextDouble() * 0.08); // 12-20%
                var monthlyPayment = CalculateMonthlyPayment(principal, interestRate, termMonths);
                
                var applicationDate = DateTime.UtcNow.AddDays(-random.Next(30, 365));
                var disbursementDate = applicationDate.AddDays(random.Next(1, 30));
                
                loans.Add(new Loan
                {
                    LoanNumber = $"LN{loanCounter:D3}",
                    LoanType = loanTypes[random.Next(loanTypes.Length)],
                    PrincipalAmount = principal,
                    InterestRate = interestRate,
                    TermInMonths = termMonths,
                    MonthlyPayment = monthlyPayment,
                    OutstandingBalance = principal * (decimal)(0.1 + random.NextDouble() * 0.9), // Random outstanding
                    ApplicationDate = applicationDate,
                    ApprovalDate = applicationDate.AddDays(random.Next(1, 14)),
                    DisbursementDate = disbursementDate,
                    MaturityDate = disbursementDate.AddMonths(termMonths),
                    Status = random.NextDouble() > 0.2 ? LoanStatus.Active : LoanStatus.Completed,
                    Purpose = "Business development loan",
                    MemberId = member.Id,
                    TenantId = member.TenantId
                });
                
                loanCounter++;
            }
        }

        return loans;
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
    {
        var monthlyRate = annualRate / 12;
        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        return principal * monthlyRate * factor / (factor - 1);
    }
}
