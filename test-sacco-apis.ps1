# SACCO Analytics Platform - Enhanced API Test Suite
param(
    [string]$BaseUrl = "http://localhost:5211/api/v1",
    [switch]$Detailed = $false
)

$headers = @{ "Content-Type" = "application/json" }
$script:AdminToken = ""
$script:TenantId = ""

Write-Host "SACCO Analytics Platform - Enhanced API Test Suite" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "=" * 60

# Enhanced API call function
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
        return @{ 
            Success = $false; 
            Error = $_.Exception.Message; 
            StatusCode = $_.Exception.Response.StatusCode.value__
        }
    }
}

# Test 1: Health Check
Write-Host "`n=== Test 1: Health Check ===" -ForegroundColor Cyan
$result = Invoke-APICall -Uri "http://localhost:5211/health"
if ($result.Success) {
    Write-Host "Health Check: PASSED" -ForegroundColor Green
    Write-Host "   Status: $($result.Data.Status)" -ForegroundColor Gray
} else {
    Write-Host "Health Check: FAILED - $($result.Error)" -ForegroundColor Red
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
    Write-Host "Admin Login: PASSED" -ForegroundColor Green
    Write-Host "   User: $($result.Data.user.firstName) $($result.Data.user.lastName)" -ForegroundColor Gray
    Write-Host "   Roles: $($result.Data.user.roles -join ', ')" -ForegroundColor Gray
} else {
    Write-Host "Admin Login: FAILED - $($result.Error)" -ForegroundColor Red
    exit 1
}

# Test 3: Get Available Tenants
Write-Host "`n=== Test 3: Get Available Tenants ===" -ForegroundColor Cyan
$result = Invoke-APICall -Uri "$BaseUrl/seed/tenants" -UseAuth $true
if ($result.Success) {
    Write-Host "Get Tenants: PASSED" -ForegroundColor Green
    Write-Host "   Available Tenants:" -ForegroundColor Gray
    foreach ($tenant in $result.Data) {
        Write-Host "   - $($tenant.name) ($($tenant.code)) - $($tenant.memberCount) members" -ForegroundColor Gray
        if (!$script:TenantId -and $tenant.memberCount -eq 0) {
            $script:TenantId = $tenant.id
            Write-Host "     Selected for testing: $($tenant.id)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "Get Tenants: FAILED - $($result.Error)" -ForegroundColor Red
}

# Test 4: Create Sample Financial Data
if ($script:TenantId) {
    Write-Host "`n=== Test 4: Create Sample Financial Data ===" -ForegroundColor Cyan
    $result = Invoke-APICall -Uri "$BaseUrl/seed/financial-data/$($script:TenantId)" -Method POST -UseAuth $true
    if ($result.Success) {
        Write-Host "Create Sample Data: PASSED" -ForegroundColor Green
        Write-Host "   Tenant: $($result.Data.tenantName)" -ForegroundColor Gray
        Write-Host "   Members: $($result.Data.data.members)" -ForegroundColor Gray
        Write-Host "   Accounts: $($result.Data.data.accounts)" -ForegroundColor Gray
        Write-Host "   Transactions: $($result.Data.data.transactions)" -ForegroundColor Gray
        Write-Host "   Loans: $($result.Data.data.loans)" -ForegroundColor Gray
    } else {
        if ($result.StatusCode -eq 400) {
            Write-Host "Sample Data: ALREADY EXISTS (skipping)" -ForegroundColor Yellow
        } else {
            Write-Host "Create Sample Data: FAILED" -ForegroundColor Red
            Write-Host "   Error: $($result.Error)" -ForegroundColor Red
            Write-Host "   Status Code: $($result.StatusCode)" -ForegroundColor Red
            Write-Host "   Check your backend console for detailed error logs" -ForegroundColor Yellow
        }
    }
}

# Test 5: Dashboard Summary Report
Write-Host "`n=== Test 5: Dashboard Summary Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/dashboard-summary?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "Dashboard Summary: PASSED" -ForegroundColor Green
        $summary = $result.Data
        Write-Host "   SACCO Financial Summary:" -ForegroundColor Gray
        Write-Host "   Total Members: $($summary.totalMembers)" -ForegroundColor Gray
        Write-Host "   New Members (30 days): $($summary.newMembersThisMonth)" -ForegroundColor Gray
        Write-Host "   Total Savings: KSh $('{0:N0}' -f $summary.totalSavings)" -ForegroundColor Gray
        Write-Host "   Total Shares: KSh $('{0:N0}' -f $summary.totalShares)" -ForegroundColor Gray
        Write-Host "   Loans Outstanding: KSh $('{0:N0}' -f $summary.totalLoansOutstanding)" -ForegroundColor Gray
        Write-Host "   Recent Deposits (30d): KSh $('{0:N0}' -f $summary.recentDeposits)" -ForegroundColor Gray
        Write-Host "   Recent Withdrawals (30d): KSh $('{0:N0}' -f $summary.recentWithdrawals)" -ForegroundColor Gray
    } else {
        Write-Host "Dashboard Summary: FAILED - $($result.Error)" -ForegroundColor Red
    }
}

# Test 6: Members Report
Write-Host "`n=== Test 6: Members Report ===" -ForegroundColor Cyan
if ($script:TenantId) {
    $result = Invoke-APICall -Uri "$BaseUrl/reports/members?tenantId=$($script:TenantId)" -UseAuth $true
    if ($result.Success) {
        Write-Host "Members Report: PASSED" -ForegroundColor Green
        Write-Host "   Member Details:" -ForegroundColor Gray
        foreach ($member in $result.Data | Select-Object -First 3) {
            Write-Host "   - $($member.memberNumber): $($member.fullName)" -ForegroundColor Gray
            Write-Host "     Email: $($member.email)" -ForegroundColor Gray
            Write-Host "     Savings: KSh $('{0:N0}' -f $member.totalSavings) | Shares: KSh $('{0:N0}' -f $member.totalShares) | Loans: KSh $('{0:N0}' -f $member.activeLoans)" -ForegroundColor Gray
        }
        if ($result.Data.Count -gt 3) {
            Write-Host "   ... and $($result.Data.Count - 3) more members" -ForegroundColor Gray
        }
    } else {
        Write-Host "Members Report: FAILED - $($result.Error)" -ForegroundColor Red
    }
}

# Summary
Write-Host "`n" + "=" * 60
Write-Host "API TEST SUITE COMPLETED" -ForegroundColor Green

if ($script:TenantId) {
    Write-Host "`nTest Environment:" -ForegroundColor Cyan
    Write-Host "Tenant ID: $($script:TenantId)" -ForegroundColor Gray
    Write-Host "Use this tenant ID to test your frontend components." -ForegroundColor Gray
}
