# =====================================================
# Inspect Existing Entity Structure
# Run this to see what entities/properties you actually have
# =====================================================

Write-Host "Inspecting your existing entity structure..." -ForegroundColor Cyan

# Navigate to backend
Set-Location backend

# Find all entity files
Write-Host "`n1. Looking for Entity files..." -ForegroundColor Yellow
$entityFiles = Get-ChildItem -Recurse -Filter "*.cs" | Where-Object { 
    $_.Directory.Name -like "*Entit*" -or 
    $_.Directory.Name -like "*Domain*" -or 
    $_.Directory.Name -like "*Models*" -or
    $_.Name -like "*Member*" -or 
    $_.Name -like "*Loan*" -or 
    $_.Name -like "*Transaction*"
}

if ($entityFiles) {
    Write-Host "Found entity files:" -ForegroundColor Green
    foreach ($file in $entityFiles) {
        Write-Host "  - $($file.FullName.Replace($PWD, '.'))" -ForegroundColor Gray
    }
} else {
    Write-Host "No entity files found. Let's check the Infrastructure project..." -ForegroundColor Yellow
}

# Look for DbContext to understand entity structure
Write-Host "`n2. Looking for DbContext..." -ForegroundColor Yellow
$dbContextFiles = Get-ChildItem -Recurse -Filter "*DbContext*.cs"

foreach ($file in $dbContextFiles) {
    Write-Host "`nFound DbContext: $($file.Name)" -ForegroundColor Green
    Write-Host "Path: $($file.FullName.Replace($PWD, '.'))" -ForegroundColor Gray
    
    # Read and show DbSet definitions
    $content = Get-Content $file.FullName
    $dbSetLines = $content | Where-Object { $_ -like "*DbSet*" }
    
    if ($dbSetLines) {
        Write-Host "DbSet definitions:" -ForegroundColor Cyan
        foreach ($line in $dbSetLines) {
            Write-Host "  $line" -ForegroundColor White
        }
    }
}

# Look for any existing controllers to understand the pattern
Write-Host "`n3. Looking for existing controllers..." -ForegroundColor Yellow
$controllerFiles = Get-ChildItem -Recurse -Filter "*Controller*.cs" | Where-Object { 
    $_.Name -notlike "*v1*" 
}

if ($controllerFiles) {
    Write-Host "Found existing controllers:" -ForegroundColor Green
    foreach ($file in $controllerFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Gray
        
        # Show first few lines to understand structure
        $content = Get-Content $file.FullName -TotalCount 20
        $classLine = $content | Where-Object { $_ -like "*class*Controller*" } | Select-Object -First 1
        if ($classLine) {
            Write-Host "    $classLine" -ForegroundColor White
        }
    }
}

# Check project references
Write-Host "`n4. Checking project references..." -ForegroundColor Yellow
$csprojFiles = Get-ChildItem -Recurse -Filter "*.csproj"

foreach ($file in $csprojFiles) {
    if ($file.Name -like "*API*") {
        Write-Host "`nAPI Project references:" -ForegroundColor Cyan
        $content = Get-Content $file.FullName
        $references = $content | Where-Object { $_ -like "*ProjectReference*" }
        foreach ($ref in $references) {
            Write-Host "  $ref" -ForegroundColor Gray
        }
    }
}

Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "ENTITY INSPECTION COMPLETED" -ForegroundColor Cyan
Write-Host "="*60 -ForegroundColor Cyan

Write-Host "`nNext: Based on the output above, we'll create controllers that match your actual entities." -ForegroundColor Yellow