# =====================================================
# SACCO Analytics Platform - Backend Foundation Tests  
# CORRECTED VERSION - Handles different URL patterns properly
# =====================================================

param(
    [string]$BaseUrl = "http://localhost:5211",
    [string]$ApiPrefix = "api/v1",
    [string]$TenantId = "219be8a6-2175-4beb-b4c9-875353e62810"
)

# Global variables
$Global:AuthToken = $null
$Global:TestResults = @()

# Helper Functions
function Write-TestHeader($title) {
    Write-Host "`n" + "="*60 -ForegroundColor Cyan
    Write-Host "=== $title ===" -ForegroundColor Cyan
    Write-Host "="*60 -ForegroundColor Cyan
}

function Write-TestResult($testName, $status, $details = "") {
    $color = if ($status -eq "PASSED") { "Green" } elseif ($status -eq "SKIPPED") { "Yellow" } else { "Red" }
    Write-Host "$testName`: $status" -ForegroundColor $color
    if ($details) { Write-Host "   $details" -ForegroundColor Gray }
    
    $Global:TestResults += [PSCustomObject]@{
        Test = $testName
        Status = $status
        Details = $details
        Timestamp = Get-Date
    }
}

# =====================================================
# TEST 1: BASIC CONNECTIVITY & AUTH
# =====================================================
Write-TestHeader "Basic Connectivity & Authentication"

Write-Host "Testing Configuration:" -ForegroundColor Gray
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "API Endpoints: $BaseUrl/$ApiPrefix" -ForegroundColor Gray
Write-Host "Health Endpoint: $BaseUrl/health" -ForegroundColor Gray

# Test 1.1: Health Check (Root level endpoint)
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 10
    
    if ($health.Status -eq "Healthy") {
        Write-TestResult "Health Check" "PASSED" "Status: $($health.Status)"
    } else {
        Write-TestResult "Health Check" "FAILED" "Unexpected status: $($health.Status)"
    }
}
catch {
    Write-TestResult "Health Check" "FAILED" "Cannot connect: $($_.Exception.Message)"
    Write-Host "   Make sure your backend is running with: dotnet run --project src/SaccoAnalytics.API" -ForegroundColor Yellow
    exit 1
}

# Test 1.2: Authentication (API level endpoint)
try {
    $loginData = @{
        email = "admin@saccoanalytics.com"
        password = "Admin123!"
    } | ConvertTo-Json

    $authResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/auth/login" -Method POST -Body $loginData -ContentType "application/json" -TimeoutSec 10
    
    if ($authResponse.success -and $authResponse.accessToken) {
        $Global:AuthToken = $authResponse.accessToken
        Write-TestResult "Admin Login" "PASSED" "User: $($authResponse.user.firstName) $($authResponse.user.lastName)"
    } elseif ($authResponse.accessToken) {
        # Handle case where success field might not be present
        $Global:AuthToken = $authResponse.accessToken
        Write-TestResult "Admin Login" "PASSED" "Authentication successful"
    } else {
        Write-TestResult "Admin Login" "FAILED" "No access token received"
    }
}
catch {
    Write-TestResult "Admin Login" "FAILED" "Auth failed: $($_.Exception.Message)"
    Write-Host "   Check your credentials: admin@saccoanalytics.com / Admin123!" -ForegroundColor Yellow
}

# Test 1.3: Dashboard Summary (Known working endpoint)
if ($Global:AuthToken) {
    try {
        $headers = @{
            'Authorization' = "Bearer $Global:AuthToken"
            'Content-Type' = 'application/json'
        }
        
        $dashboard = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/reports/dashboard-summary?tenantId=$TenantId" -Method GET -Headers $headers -TimeoutSec 10
        
        if ($dashboard -and ($dashboard.success -ne $false)) {
            Write-TestResult "Dashboard Summary" "PASSED" "Total Members: $($dashboard.totalMembers)"
        } else {
            Write-TestResult "Dashboard Summary" "FAILED" "Unexpected response format"
        }
    }
    catch {
        Write-TestResult "Dashboard Summary" "FAILED" "Dashboard error: $($_.Exception.Message)"
    }
} else {
    Write-TestResult "Dashboard Summary" "SKIPPED" "No auth token available"
}

# =====================================================
# TEST 2: MEMBER MANAGEMENT ENDPOINTS (TO BE BUILT)
# =====================================================
Write-TestHeader "Member Management CRUD Endpoints"

if (-not $Global:AuthToken) {
    Write-TestResult "Member CRUD Tests" "SKIPPED" "No authentication token"
} else {
    $headers = @{
        'Authorization' = "Bearer $Global:AuthToken"
        'Content-Type' = 'application/json'
    }
    
    # Test 2.1: List Members (GET)
    try {
        $listResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/members?tenantId=$TenantId" -Method GET -Headers $headers -TimeoutSec 5
        Write-TestResult "GET /members" "PASSED" "Members endpoint exists"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "GET /members" "SKIPPED" "Endpoint not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "GET /members" "FAILED" $_.Exception.Message
        }
    }
    
    # Test 2.2: Create Member (POST)
    $newMember = @{
        memberNumber = "M$(Get-Random -Minimum 1000 -Maximum 9999)"
        firstName = "John"
        lastName = "Doe"
        email = "john.doe.$(Get-Random)@email.com"
        phoneNumber = "+254712$(Get-Random -Minimum 100000 -Maximum 999999)"
        idNumber = "$(Get-Random -Minimum 10000000 -Maximum 99999999)"
        dateOfBirth = "1985-06-15"
        initialDeposit = 5000.00
        tenantId = $TenantId
    } | ConvertTo-Json
    
    try {
        $createResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/members" -Method POST -Body $newMember -Headers $headers -TimeoutSec 5
        
        if ($createResponse.success -and $createResponse.data.id) {
            Write-TestResult "POST /members" "PASSED" "Member created: $($createResponse.data.id)"
            $memberId = $createResponse.data.id
            
            # Test 2.3: Get Single Member (GET by ID)
            try {
                $getMember = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/members/$memberId" -Method GET -Headers $headers -TimeoutSec 5
                if ($getMember.success) {
                    Write-TestResult "GET /members/{id}" "PASSED" "Member retrieved successfully"
                } else {
                    Write-TestResult "GET /members/{id}" "FAILED" "Could not retrieve member"
                }
            }
            catch {
                if ($_.Exception.Message -like "*404*") {
                    Write-TestResult "GET /members/{id}" "SKIPPED" "Get by ID not implemented"
                } else {
                    Write-TestResult "GET /members/{id}" "FAILED" $_.Exception.Message
                }
            }
            
            # Test 2.4: Update Member (PUT)
            $updateData = @{
                firstName = "Jane"
                lastName = "Smith"
            } | ConvertTo-Json
            
            try {
                $updateResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/members/$memberId" -Method PUT -Body $updateData -Headers $headers -TimeoutSec 5
                if ($updateResponse.success) {
                    Write-TestResult "PUT /members/{id}" "PASSED" "Member updated successfully"
                } else {
                    Write-TestResult "PUT /members/{id}" "FAILED" "Update failed"
                }
            }
            catch {
                if ($_.Exception.Message -like "*404*") {
                    Write-TestResult "PUT /members/{id}" "SKIPPED" "Update endpoint not implemented"
                } else {
                    Write-TestResult "PUT /members/{id}" "FAILED" $_.Exception.Message
                }
            }
            
        } else {
            Write-TestResult "POST /members" "FAILED" "Create member failed"
        }
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "POST /members" "SKIPPED" "Create endpoint not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "POST /members" "FAILED" $_.Exception.Message
        }
    }
    
    # Test 2.5: Search Members
    try {
        $searchResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/members/search?tenantId=$TenantId&query=John" -Method GET -Headers $headers -TimeoutSec 5
        Write-TestResult "GET /members/search" "PASSED" "Search endpoint working"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "GET /members/search" "SKIPPED" "Search not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "GET /members/search" "FAILED" $_.Exception.Message
        }
    }
}

# =====================================================
# TEST 3: TRANSACTION MANAGEMENT ENDPOINTS
# =====================================================
Write-TestHeader "Transaction Management CRUD Endpoints"

if (-not $Global:AuthToken) {
    Write-TestResult "Transaction CRUD Tests" "SKIPPED" "No authentication token"
} else {
    $headers = @{
        'Authorization' = "Bearer $Global:AuthToken"
        'Content-Type' = 'application/json'
    }
    
    # Test 3.1: List Transactions
    try {
        $transactionsResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/transactions?tenantId=$TenantId" -Method GET -Headers $headers -TimeoutSec 5
        Write-TestResult "GET /transactions" "PASSED" "Transactions endpoint exists"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "GET /transactions" "SKIPPED" "Transactions endpoint not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "GET /transactions" "FAILED" $_.Exception.Message
        }
    }
    
    # Test 3.2: Create Transaction
    $newTransaction = @{
        memberId = [System.Guid]::NewGuid().ToString()
        transactionType = "Deposit"
        amount = 1500.00
        description = "Test deposit"
        accountType = "Savings"
        referenceNumber = "TXN$(Get-Random -Minimum 100000 -Maximum 999999)"
        tenantId = $TenantId
    } | ConvertTo-Json
    
    try {
        $createTransactionResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/transactions" -Method POST -Body $newTransaction -Headers $headers -TimeoutSec 5
        Write-TestResult "POST /transactions" "PASSED" "Transaction created successfully"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "POST /transactions" "SKIPPED" "Create transaction not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "POST /transactions" "FAILED" $_.Exception.Message
        }
    }
}

# =====================================================
# TEST 4: LOAN MANAGEMENT ENDPOINTS
# =====================================================
Write-TestHeader "Loan Management CRUD Endpoints"

if (-not $Global:AuthToken) {
    Write-TestResult "Loan CRUD Tests" "SKIPPED" "No authentication token"
} else {
    $headers = @{
        'Authorization' = "Bearer $Global:AuthToken"
        'Content-Type' = 'application/json'
    }
    
    # Test 4.1: List Loans
    try {
        $loansResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/loans?tenantId=$TenantId" -Method GET -Headers $headers -TimeoutSec 5
        Write-TestResult "GET /loans" "PASSED" "Loans endpoint exists"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "GET /loans" "SKIPPED" "Loans endpoint not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "GET /loans" "FAILED" $_.Exception.Message
        }
    }
    
    # Test 4.2: Create Loan Application
    $loanApplication = @{
        memberId = [System.Guid]::NewGuid().ToString()
        loanType = "Personal"
        requestedAmount = 50000.00
        purpose = "Business expansion"
        termInMonths = 12
        interestRate = 12.5
        tenantId = $TenantId
    } | ConvertTo-Json
    
    try {
        $createLoanResponse = Invoke-RestMethod -Uri "$BaseUrl/$ApiPrefix/loans" -Method POST -Body $loanApplication -Headers $headers -TimeoutSec 5
        Write-TestResult "POST /loans" "PASSED" "Loan application created successfully"
    }
    catch {
        if ($_.Exception.Message -like "*404*") {
            Write-TestResult "POST /loans" "SKIPPED" "Create loan not implemented - NEED TO BUILD"
        } else {
            Write-TestResult "POST /loans" "FAILED" $_.Exception.Message
        }
    }
}

# =====================================================
# DEVELOPMENT PROGRESS SUMMARY
# =====================================================
Write-TestHeader "Development Progress Summary"

$totalTests = $Global:TestResults.Count
$passedTests = ($Global:TestResults | Where-Object { $_.Status -eq "PASSED" }).Count
$failedTests = ($Global:TestResults | Where-Object { $_.Status -eq "FAILED" }).Count
$skippedTests = ($Global:TestResults | Where-Object { $_.Status -eq "SKIPPED" }).Count

Write-Host "`nTEST RESULTS SUMMARY:" -ForegroundColor White
Write-Host "✓ Passed: $passedTests" -ForegroundColor Green
Write-Host "✗ Failed: $failedTests" -ForegroundColor Red  
Write-Host "⚠ Skipped (Need to Build): $skippedTests" -ForegroundColor Yellow
Write-Host "━ Total Tests: $totalTests" -ForegroundColor White

if ($passedTests -gt 0) {
    Write-Host "`nWHAT'S WORKING:" -ForegroundColor Green
    $Global:TestResults | Where-Object { $_.Status -eq "PASSED" } | ForEach-Object {
        Write-Host "  ✓ $($_.Test)" -ForegroundColor Green
    }
}

if ($skippedTests -gt 0) {
    Write-Host "`nWHAT NEEDS TO BE BUILT:" -ForegroundColor Yellow
    $Global:TestResults | Where-Object { $_.Status -eq "SKIPPED" } | ForEach-Object {
        Write-Host "  ⚠ $($_.Test)" -ForegroundColor Yellow
    }
}

if ($failedTests -gt 0) {
    Write-Host "`nWHAT'S BROKEN:" -ForegroundColor Red
    $Global:TestResults | Where-Object { $_.Status -eq "FAILED" } | ForEach-Object {
        Write-Host "  ✗ $($_.Test): $($_.Details)" -ForegroundColor Red
    }
}

# Development recommendations
Write-Host "`nDEVELOPMENT ROADMAP:" -ForegroundColor Cyan
if ($skippedTests -gt 0) {
    Write-Host "PRIORITY 1: Implement Member Management CRUD" -ForegroundColor White
    Write-Host "  - Create MembersController.cs in your Controllers folder" -ForegroundColor Gray
    Write-Host "  - Add GET, POST, PUT endpoints for /api/v1/members" -ForegroundColor Gray
    
    Write-Host "`nPRIORITY 2: Implement Transaction Management" -ForegroundColor White
    Write-Host "  - Create TransactionsController.cs" -ForegroundColor Gray
    Write-Host "  - Add CRUD endpoints for /api/v1/transactions" -ForegroundColor Gray
    
    Write-Host "`nPRIORITY 3: Implement Loan Management" -ForegroundColor White
    Write-Host "  - Create LoansController.cs" -ForegroundColor Gray
    Write-Host "  - Add CRUD endpoints for /api/v1/loans" -ForegroundColor Gray
    
    Write-Host "`nAfter building each controller:" -ForegroundColor Yellow
    Write-Host "  Run this script again to track progress" -ForegroundColor Gray
} else {
    Write-Host "🎉 All endpoints implemented! Ready for frontend development." -ForegroundColor Green
}

Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "BACKEND FOUNDATION ANALYSIS COMPLETED" -ForegroundColor Cyan
Write-Host "="*60 -ForegroundColor Cyan

Write-Host "`nConfiguration Details:" -ForegroundColor Gray
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "API Prefix: $ApiPrefix" -ForegroundColor Gray
Write-Host "Tenant ID: $TenantId" -ForegroundColor Gray
Write-Host "Auth Token: $(if($Global:AuthToken) {'✓ Available (' + $Global:AuthToken.Substring(0,20) + '...)'} else {'✗ Missing'})" -ForegroundColor Gray