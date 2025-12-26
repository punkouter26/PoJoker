# Check Latest .NET Package Versions with Context7
# This script queries Context7 for the latest versions of .NET packages

param(
    [Parameter(Mandatory=$false)]
    [string]$Context7ApiKey = $env:CONTEXT7_API_KEY
)

function Get-LatestPackageVersion {
    param(
        [string]$PackageName,
        [string]$Registry = "nuget"
    )

    Write-Host "Checking latest version for: $PackageName" -ForegroundColor Cyan

    # Use npx to run context7-mcp and query for the package
    $result = npx @upstash/context7-mcp query "$Registry:$PackageName" 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Latest version: $result" -ForegroundColor Green
        return $result
    } else {
        Write-Host "  Unable to fetch version" -ForegroundColor Yellow
        return $null
    }
}

Write-Host "`n=== Checking Latest .NET Package Versions ===" -ForegroundColor Magenta
Write-Host "Current Date: $(Get-Date -Format 'yyyy-MM-dd')`n" -ForegroundColor Gray

# Core .NET packages
$corePackages = @(
    "Microsoft.AspNetCore.Components.WebAssembly",
    "Microsoft.AspNetCore.Components.WebAssembly.Server",
    "Microsoft.Extensions.Http.Resilience",
    "Microsoft.AspNetCore.Mvc.Testing"
)

# Aspire packages
$aspirePackages = @(
    "Aspire.Hosting.AppHost",
    "Aspire.Hosting.Azure.Storage",
    "Aspire.Azure.Data.Tables",
    "Microsoft.Extensions.ServiceDiscovery"
)

# Azure packages
$azurePackages = @(
    "Azure.AI.OpenAI",
    "Azure.Data.Tables",
    "Azure.Identity",
    "Azure.Monitor.OpenTelemetry.Exporter"
)

# Testing packages
$testingPackages = @(
    "Microsoft.NET.Test.Sdk",
    "xunit",
    "xunit.runner.visualstudio",
    "FluentAssertions",
    "Moq",
    "Microsoft.Playwright"
)

Write-Host "`n--- Core .NET Packages ---" -ForegroundColor Yellow
foreach ($package in $corePackages) {
    Get-LatestPackageVersion -PackageName $package
}

Write-Host "`n--- Aspire Packages ---" -ForegroundColor Yellow
foreach ($package in $aspirePackages) {
    Get-LatestPackageVersion -PackageName $package
}

Write-Host "`n--- Azure Packages ---" -ForegroundColor Yellow
foreach ($package in $azurePackages) {
    Get-LatestPackageVersion -PackageName $package
}

Write-Host "`n--- Testing Packages ---" -ForegroundColor Yellow
foreach ($package in $testingPackages) {
    Get-LatestPackageVersion -PackageName $package
}

Write-Host "`n=== Version Check Complete ===" -ForegroundColor Magenta
Write-Host "To update packages, modify Directory.Packages.props with the latest versions." -ForegroundColor Gray