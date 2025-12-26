# Context7 MCP Integration

## Overview

Context7 is a Model Context Protocol (MCP) server that provides real-time package version information for various package managers including NuGet, npm, Maven, and more. This integration allows our project to automatically query and track the latest versions of .NET packages.

## Installation

The Context7 MCP server has been installed globally via npm:

```bash
npm install -g @upstash/context7-mcp
```

## Configuration

### Claude Code Configuration

To use Context7 with Claude Code, configure it in your Claude settings:

1. Copy the MCP configuration from `claude_mcp_config.json`
2. Add your Context7 API key (get one from https://context7.com)
3. Restart Claude Code to load the MCP server

### Environment Setup

Set your Context7 API key as an environment variable:

```powershell
$env:CONTEXT7_API_KEY = "your-api-key-here"
```

Or add it to your system environment variables permanently.

## Usage

### PowerShell Script

Check for the latest versions of all project packages:

```powershell
.\scripts\check-dotnet-versions.ps1
```

### Node.js Script

Run the package version checker with detailed categorization:

```bash
node scripts/update-packages.js
```

### Direct Context7 Queries

Query specific packages directly:

```bash
# Query a NuGet package
npx @upstash/context7-mcp query nuget:Microsoft.AspNetCore.Components.WebAssembly

# Query an npm package
npx @upstash/context7-mcp query npm:@upstash/context7-mcp
```

## Integration with CI/CD

The Context7 integration is referenced in our copilot instructions (`.github/workflows/copilot-instructions.md`) to ensure that AI assistants use the latest package versions when suggesting updates.

### GitHub Actions Integration

You can add a GitHub Action to check for outdated packages:

```yaml
name: Check Package Versions
on:
  schedule:
    - cron: '0 0 * * 1' # Weekly on Monday
  workflow_dispatch:

jobs:
  check-versions:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm install -g @upstash/context7-mcp
      - run: node scripts/update-packages.js
```

## Package Categories

Our project tracks packages in the following categories:

### Core .NET
- Microsoft.AspNetCore.Components.WebAssembly
- Microsoft.AspNetCore.Components.WebAssembly.Server
- Microsoft.Extensions.Http.Resilience

### Aspire
- Aspire.Hosting.AppHost
- Aspire.Hosting.Azure.Storage
- Aspire.Azure.Data.Tables

### Azure Services
- Azure.AI.OpenAI
- Azure.Data.Tables
- Azure.Identity

### Testing
- Microsoft.NET.Test.Sdk
- xunit
- FluentAssertions
- Moq

## Benefits

1. **Real-time Updates**: Get the latest package versions without manual checking
2. **Automated Tracking**: Scripts automatically categorize and check all packages
3. **AI Integration**: Claude and other AI assistants can query Context7 for accurate version information
4. **Central Management**: All package versions managed in `Directory.Packages.props`

## Troubleshooting

### Context7 API Key Issues
- Ensure your API key is set in the environment
- Check that the key has proper permissions at https://context7.com

### MCP Server Not Loading
- Restart Claude Code after configuration changes
- Check the MCP logs in Claude Code's developer console

### Package Not Found
- Verify the package name is correct
- Check if the package exists in the specified registry (nuget, npm, etc.)

## Further Resources

- [Context7 Documentation](https://context7.com/docs)
- [MCP Protocol Specification](https://modelcontextprotocol.io)
- [Upstash Context7 MCP GitHub](https://github.com/upstash/context7-mcp)