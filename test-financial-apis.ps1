# SACCO Analytics Platform - Complete API Test Suite
param(
    [string]$BaseUrl = "http://localhost:5211/api/v1",
    [switch]$Detailed = $false
)

$headers = @{ "Content-Type" = "application/json" }
$script:AdminToken = ""
$script:TenantId = ""

Write-Host "🚀 SACCO Analytics Platform - API Test Suite" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "=" * 60

# Helper function to make API calls
function Invoke-APICall {
    param($Uri, $Method = "GET", $Body = $null, $UseAuth = $false)
    
    $callHeaders = $headers.Clone()
    if ($UseAuth -and $script:AdminToken) {
        $callHeaders["Authorization"] = "Bearer $($script:AdminToken)"
    }
    
    try {
        if ($Body) {
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Body $Body -Headers $callHeaders
        } else {
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Headers $callHeaders
        }
        return @{ Success = $true; Data = $response }
    } catch {
        return @{ Success = $false; Error = $_.Exception.Message; StatusCode = $_.Exception.Response.StatusCode }
    }
}

# Test 1: Health Check
Write-Host "`n=== Test 1: Health Check ===" -ForegroundColor Cyan
$result = Invoke-APICall -Uri "http://localhost:5211/health"
if ($result.Success) {
    Write-Host "✅ Health Check: PASSED" -ForegroundColor Green
    Write-Host "   Status: $($result.Data.Status)" -ForegroundColor Gray
} else {
    Write-Host "❌ Health Check: FAILED - $($result.Error)" -ForegroundColor Red
    exit 1
}

# Test 2: Admin Login
Write-Host "`n=== Test 2: Admin Login ===" -ForegroundColor Cyan
$loginBody = @{
    email = "admin@saccoanalytics.com"
    password = "Admin123!"
} | ConvertTo-Json

$result = Invoke-APICall -Uri "$BaseUrl/auth/login" -Method POST -Body $loginBody
if ($result.Success) {
    $script:AdminToken = $result.Data.accessToken
    Write-Host "✅ Admin Login: PASSED" -ForegroundColor Green
    Write-Host "   User: $($result.Data.user.firstName) $($result.Data.user.lastName)" -ForegroundColor Gray
    Write-Host "   Roles: $($result.Data.user.roles -join ', ')" -ForegroundColor Gray
} else {
    Write-Host "❌ Admin Login: FAILED - $($result.Error)" -ForegroundColor Red
    exit 1
}

# Test 3: Get Available Tenants
Write-Host "`n=== Test 3: Get Available Tenants ===" -ForegroundColor Cyan
$result = Invoke-APICall -Uri "$BaseUrl/seed/tenants" -UseAuth $true
if ($result.Success) {
    Write-Host "✅ Get Tenants: PASSED" -ForegroundColor Green
    Write-Host "   Available Tenants:" -ForegroundColor Gray
    foreach ($tenant in $result.Data) {
        Write-Host "   - $($tenant.name) ($($tenant.code)) - $($tenant.memberCount) members" -ForegroundColor Gray
        if (!$script:TenantId -and $tenant.memberCount -eq 0) {
            $script:TenantId = $tenant.id
            Write-Host "     → Selected for testing: $($tenant.id)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "❌ Get Tenants: FAILED - $($result.Error)" -ForegroundColor Red
}

# Test 4: Create Sample Financial Data
if ($script:TenantId) {
    Write-Host "`n=== Test 4: Create Sample Financial Data ===" -ForegroundColor Cyan
    $result = Invoke-APICall -Uri "$BaseUrl/seed/financial-data/$($script:TenantId)" -Method POST -UseAuth $true
    if ($result.Success) {
        Write-Host "✅ Create Sample Data: PASSED" -ForegroundColor Green
        Write-Host "   Tenant: $($result.Data.tenantName)" -ForegroundColor Gray
        Write-Host "   Members: $($result.Data.data.members)" -ForegroundColor Gray
        Write-Host "   Accounts: $($result.Data.data.accounts)" -ForegroundColor Gray
        Write-Host "   Transactions: $($result.Data.data.transactions)" -ForegroundColor Gray
        Write-Host "   Loans: $($result.Data.data.loans)" -ForegroundColor Gray
    } else {
        if ($result.Error -like "*already exists*") {
            Write-Host "⚠️  Sample Data: ALREADY EXISTS" -ForegroundColor Yellow
        } else {
            Write-Host "❌ Create Sample Data: FAILED - $($result.Error)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "`n=== Test 4: Create Sample Financial Data ===" -ForegroundColor Cyan
    Write-Host "⚠️  Skipped - No tenant available for testing" -ForegroundColor Yellow
}

# Test 5: Dashboard Summary Report
Write-Host "`n=== Test 5: Dashboard Summary Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/dashboard-summary?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "✅ Dashboard Summary: PASSED" -ForegroundColor Green
        $summary = $result.Data
        Write-Host "   📊 SACCO Summary:" -ForegroundColor Gray
        Write-Host "   Total Members: $($summary.totalMembers)" -ForegroundColor Gray
        Write-Host "   New Members (30 days): $($summary.newMembersThisMonth)" -ForegroundColor Gray
        Write-Host "   Total Savings: KSh $('{0:N0}' -f $summary.totalSavings)" -ForegroundColor Gray
        Write-Host "   Total Shares: KSh $('{0:N0}' -f $summary.totalShares)" -ForegroundColor Gray
        Write-Host "   Loans Outstanding: KSh $('{0:N0}' -f $summary.totalLoansOutstanding)" -ForegroundColor Gray
        Write-Host "   Recent Deposits (30d): KSh $('{0:N0}' -f $summary.recentDeposits)" -ForegroundColor Gray
        Write-Host "   Recent Withdrawals (30d): KSh $('{0:N0}' -f $summary.recentWithdrawals)" -ForegroundColor Gray
    } else {
        Write-Host "❌ Dashboard Summary: FAILED - $($result.Error)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  Skipped - No tenant ID available" -ForegroundColor Yellow
}

# Test 6: Members Report
Write-Host "`n=== Test 6: Members Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/members?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "✅ Members Report: PASSED" -ForegroundColor Green
        Write-Host "   👥 Member Details:" -ForegroundColor Gray
        foreach ($member in $result.Data | Select-Object -First 3) {
            Write-Host "   - $($member.memberNumber): $($member.fullName)" -ForegroundColor Gray
            Write-Host "     Email: $($member.email)" -ForegroundColor Gray
            Write-Host "     Savings: KSh $('{0:N0}' -f $member.totalSavings) | Shares: KSh $('{0:N0}' -f $member.totalShares) | Loans: KSh $('{0:N0}' -f $member.activeLoans)" -ForegroundColor Gray
        }
        if ($result.Data.Count -gt 3) {
            Write-Host "   ... and $($result.Data.Count - 3) more members" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ Members Report: FAILED - $($result.Error)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  Skipped - No tenant ID available" -ForegroundColor Yellow
}

# Test 7: Transactions Report
Write-Host "`n=== Test 7: Transactions Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/transactions?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "✅ Transactions Report: PASSED" -ForegroundColor Green
        Write-Host "   💰 Recent Transactions (showing first 5):" -ForegroundColor Gray
        foreach ($txn in $result.Data | Select-Object -First 5) {
            $date = ([datetime]$txn.transactionDate).ToString("MMM dd, yyyy")
            Write-Host "   - $($txn.transactionReference): $($txn.transactionType) KSh $('{0:N0}' -f $txn.amount)" -ForegroundColor Gray
            Write-Host "     Member: $($txn.memberName) | Account: $($txn.accountNumber) | Date: $date" -ForegroundColor Gray
        }
        Write-Host "   Total Transactions Retrieved: $($result.Data.Count)" -ForegroundColor Gray
    } else {
        Write-Host "❌ Transactions Report: FAILED - $($result.Error)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  Skipped - No tenant ID available" -ForegroundColor Yellow
}

# Test 8: Loans Report
Write-Host "`n=== Test 8: Loans Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/loans?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "✅ Loans Report: PASSED" -ForegroundColor Green
        Write-Host "   🏦 Loan Details:" -ForegroundColor Gray
        foreach ($loan in $result.Data | Select-Object -First 3) {
            Write-Host "   - $($loan.loanNumber): $($loan.memberName) - $($loan.loanType)" -ForegroundColor Gray
            Write-Host "     Principal: KSh $('{0:N0}' -f $loan.principalAmount) | Outstanding: KSh $('{0:N0}' -f $loan.outstandingBalance)" -ForegroundColor Gray
            Write-Host "     Status: $($loan.status) | Rate: $($loan.interestRate * 100)%" -ForegroundColor Gray
        }
        if ($result.Data.Count -gt 3) {
            Write-Host "   ... and $($result.Data.Count - 3) more loans" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ Loans Report: FAILED - $($result.Error)" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  Skipped - No tenant ID available" -ForegroundColor Yellow
}

# Test 9: Regular User Login
Write-Host "`n=== Test 9: Regular User Login ===" -ForegroundColor Cyan
$userLoginBody = @{
    email = "test@example.com"
    password = "TestPassword123"
} | ConvertTo-Json

$result = Invoke-APICall -Uri "$BaseUrl/auth/login" -Method POST -Body $userLoginBody
if ($result.Success) {
    Write-Host "✅ Regular User Login: PASSED" -ForegroundColor Green
    Write-Host "   User: $($result.Data.user.firstName) $($result.Data.user.lastName)" -ForegroundColor Gray
    Write-Host "   Roles: $($result.Data.user.roles -join ', ')" -ForegroundColor Gray
    if ($result.Data.user.tenantId) {
        Write-Host "   Tenant: $($result.Data.user.tenantId)" -ForegroundColor Gray
    } else {
        Write-Host "   Tenant: Not assigned" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Regular User Login: FAILED - $($result.Error)" -ForegroundColor Red
}

# Summary
Write-Host "`n" + "=" * 60
Write-Host "🎉 API TEST SUITE COMPLETED" -ForegroundColor Green
Write-Host "All core APIs have been tested with realistic SACCO data!" -ForegroundColor Yellow

if ($script:TenantId) {
    Write-Host "`n📋 Test Data Created:" -ForegroundColor Cyan
    Write-Host "Tenant ID: $($script:TenantId)" -ForegroundColor Gray
    Write-Host "You can now use this data to test the frontend components." -ForegroundColor Gray
}

Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Test the frontend with this data" -ForegroundColor Gray
Write-Host "2. Build reporting dashboards" -ForegroundColor Gray
Write-Host "3. Add export functionality" -ForegroundColor Gray
