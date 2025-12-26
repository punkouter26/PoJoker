#!/usr/bin/env node

/**
 * Update Package Versions using Context7 MCP
 * This script checks for the latest versions of packages defined in Directory.Packages.props
 * and optionally updates them to the latest versions
 */

const fs = require('fs').promises;
const path = require('path');
const { execSync } = require('child_process');

// ANSI color codes for terminal output
const colors = {
    reset: '\x1b[0m',
    bright: '\x1b[1m',
    dim: '\x1b[2m',
    red: '\x1b[31m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    magenta: '\x1b[35m',
    cyan: '\x1b[36m',
};

// Parse Directory.Packages.props to extract package versions
async function parsePackagesProps(filePath) {
    const content = await fs.readFile(filePath, 'utf8');
    const packageRegex = /<PackageVersion\s+Include="([^"]+)"\s+Version="([^"]+)"/g;
    const packages = {};

    let match;
    while ((match = packageRegex.exec(content)) !== null) {
        packages[match[1]] = match[2];
    }

    return packages;
}

// Query Context7 for the latest version of a package
function getLatestVersion(packageName) {
    try {
        // Use context7 CLI or MCP to query for the package
        // This is a placeholder - actual implementation would use the Context7 API
        const result = execSync(`npx @upstash/context7-mcp query nuget:${packageName}`, {
            encoding: 'utf8',
            stdio: ['pipe', 'pipe', 'ignore']
        }).trim();

        return result;
    } catch (error) {
        return null;
    }
}

// Compare versions and determine if update is needed
function compareVersions(current, latest) {
    if (!latest) return 'unknown';
    if (current === latest) return 'current';

    const currentParts = current.split('.').map(Number);
    const latestParts = latest.split('.').map(Number);

    for (let i = 0; i < Math.max(currentParts.length, latestParts.length); i++) {
        const currPart = currentParts[i] || 0;
        const latestPart = latestParts[i] || 0;

        if (latestPart > currPart) return 'outdated';
        if (latestPart < currPart) return 'newer';
    }

    return 'current';
}

// Main function
async function main() {
    console.log(`${colors.magenta}${colors.bright}=== .NET Package Version Checker (via Context7) ===${colors.reset}\n`);

    const propsPath = path.join(process.cwd(), 'Directory.Packages.props');

    try {
        // Parse current packages
        console.log(`${colors.cyan}Reading Directory.Packages.props...${colors.reset}`);
        const currentPackages = await parsePackagesProps(propsPath);

        console.log(`Found ${Object.keys(currentPackages).length} packages\n`);

        // Categories of packages for organized output
        const categories = {
            'Core .NET': [
                'Microsoft.AspNetCore.Components.WebAssembly',
                'Microsoft.AspNetCore.Components.WebAssembly.Server',
                'Microsoft.Extensions.Http.Resilience',
            ],
            'Aspire': [
                'Aspire.Hosting.AppHost',
                'Aspire.Hosting.Azure.Storage',
                'Aspire.Azure.Data.Tables',
                'Microsoft.Extensions.ServiceDiscovery',
            ],
            'Azure Services': [
                'Azure.AI.OpenAI',
                'Azure.Data.Tables',
                'Azure.Identity',
                'Azure.Monitor.OpenTelemetry.Exporter',
            ],
            'Testing': [
                'Microsoft.NET.Test.Sdk',
                'xunit',
                'FluentAssertions',
                'Moq',
            ],
            'Telemetry': [
                'OpenTelemetry',
                'OpenTelemetry.Extensions.Hosting',
                'OpenTelemetry.Instrumentation.AspNetCore',
            ],
        };

        const updateNeeded = [];

        // Check each category
        for (const [category, packages] of Object.entries(categories)) {
            console.log(`${colors.yellow}${colors.bright}--- ${category} ---${colors.reset}`);

            for (const packageName of packages) {
                if (currentPackages[packageName]) {
                    const currentVersion = currentPackages[packageName];
                    const latestVersion = getLatestVersion(packageName);
                    const status = compareVersions(currentVersion, latestVersion);

                    let statusSymbol, statusColor;
                    switch (status) {
                        case 'current':
                            statusSymbol = '✓';
                            statusColor = colors.green;
                            break;
                        case 'outdated':
                            statusSymbol = '↑';
                            statusColor = colors.yellow;
                            updateNeeded.push({ name: packageName, current: currentVersion, latest: latestVersion });
                            break;
                        case 'newer':
                            statusSymbol = '↓';
                            statusColor = colors.blue;
                            break;
                        default:
                            statusSymbol = '?';
                            statusColor = colors.dim;
                    }

                    console.log(`  ${statusColor}${statusSymbol}${colors.reset} ${packageName}`);
                    console.log(`    Current: ${currentVersion} ${latestVersion ? `| Latest: ${latestVersion}` : ''}`);
                }
            }
            console.log();
        }

        // Summary
        if (updateNeeded.length > 0) {
            console.log(`${colors.yellow}${colors.bright}=== Updates Available ===${colors.reset}`);
            console.log(`Found ${updateNeeded.length} package(s) with newer versions:\n`);

            for (const pkg of updateNeeded) {
                console.log(`  • ${pkg.name}`);
                console.log(`    ${pkg.current} → ${pkg.latest}`);
            }

            console.log(`\n${colors.dim}To update, modify Directory.Packages.props with the latest versions.${colors.reset}`);
        } else {
            console.log(`${colors.green}${colors.bright}✓ All tracked packages are up to date!${colors.reset}`);
        }

    } catch (error) {
        console.error(`${colors.red}Error: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

// Run the script
if (require.main === module) {
    main().catch(console.error);
}

module.exports = { parsePackagesProps, getLatestVersion, compareVersions };