# API Testing Script for SACCO Analytics Platform

$baseUrl = "http://localhost:5211/api/v1"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "Starting API Tests..." -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow

# Test 1: Health Check
Write-Host "`n=== Test 1: Health Check ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5211/health" -Method GET
    Write-Host "✅ Health Check: PASSED" -ForegroundColor Green
    Write-Host "Status: $($response.Status)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Health Check: FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Test 2: Admin Login
Write-Host "`n=== Test 2: Admin Login ===" -ForegroundColor Cyan
$adminLoginBody = @{
    email = "admin@saccoanalytics.com"
    password = "Admin123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $adminLoginBody -Headers $headers
    $adminToken = $loginResponse.accessToken
    Write-Host "✅ Admin Login: PASSED" -ForegroundColor Green
    Write-Host "Admin User: $($loginResponse.user.firstName) $($loginResponse.user.lastName)" -ForegroundColor Gray
    Write-Host "Roles: $($loginResponse.user.roles -join ', ')" -ForegroundColor Gray
} catch {
    Write-Host "❌ Admin Login: FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Set authorization header
$authHeaders = @{ 
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $adminToken"
}

# Test 3: Create Tenant
Write-Host "`n=== Test 3: Create Tenant ===" -ForegroundColor Cyan
$tenantBody = @{
    name = "Nakuru Teachers SACCO"
    code = "NKRTS"
    description = "A cooperative for teachers in Nakuru region"
    contactEmail = "admin@nakuruteachers.sacco.ke"
    contactPhone = "+254712345678"
} | ConvertTo-Json

try {
    $tenantResponse = Invoke-RestMethod -Uri "$baseUrl/tenants" -Method POST -Body $tenantBody -Headers $authHeaders
    $tenantId = $tenantResponse.id
    Write-Host "✅ Create Tenant: PASSED" -ForegroundColor Green
    Write-Host "Tenant ID: $tenantId" -ForegroundColor Gray
    Write-Host "Tenant Name: $($tenantResponse.name)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Create Tenant: FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# Test 4: Get All Tenants
Write-Host "`n=== Test 4: Get All Tenants ===" -ForegroundColor Cyan
try {
    $tenantsResponse = Invoke-RestMethod -Uri "$baseUrl/tenants" -Method GET -Headers $authHeaders
    Write-Host "✅ Get All Tenants: PASSED" -ForegroundColor Green
    Write-Host "Total Tenants: $($tenantsResponse.Count)" -ForegroundColor Gray
    foreach ($tenant in $tenantsResponse) {
        Write-Host "  - $($tenant.name) ($($tenant.code))" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Get All Tenants: FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# Test 5: Regular User Login
Write-Host "`n=== Test 5: Regular User Login ===" -ForegroundColor Cyan
$userLoginBody = @{
    email = "test@example.com"
    password = "TestPassword123"
} | ConvertTo-Json

try {
    $userLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $userLoginBody -Headers $headers
    Write-Host "✅ Regular User Login: PASSED" -ForegroundColor Green
    Write-Host "User: $($userLoginResponse.user.firstName) $($userLoginResponse.user.lastName)" -ForegroundColor Gray
    Write-Host "Roles: $($userLoginResponse.user.roles -join ', ')" -ForegroundColor Gray
} catch {
    Write-Host "❌ Regular User Login: FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# Test 6: Assign User to Tenant
if ($tenantId -and $userLoginResponse.user.id) {
    Write-Host "`n=== Test 6: Assign User to Tenant ===" -ForegroundColor Cyan
    $assignBody = @{
        userId = $userLoginResponse.user.id
        tenantId = $tenantId
    } | ConvertTo-Json

    try {
        $assignResponse = Invoke-RestMethod -Uri "$baseUrl/users/assign-to-tenant" -Method POST -Body $assignBody -Headers $authHeaders
        Write-Host "✅ Assign User to Tenant: PASSED" -ForegroundColor Green
        Write-Host $assignResponse.message -ForegroundColor Gray
    } catch {
        Write-Host "❌ Assign User to Tenant: FAILED" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
}

Write-Host "`n=== API Tests Completed ===" -ForegroundColor Green
Write-Host "All core APIs have been tested!" -ForegroundColor Yellow
