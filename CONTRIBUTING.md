# Contributing to Po.Joker 🃏

Thank you for your interest in contributing to Po.Joker! We welcome contributions from the community.

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce** the behavior
- **Expected behavior**
- **Actual behavior**
- **Screenshots** if applicable
- **Environment details** (.NET version, OS, browser)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- **Clear title and description**
- **Use case** and why this would be useful
- **Possible implementation** if you have ideas

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** outlined below
3. **Write or update tests** for your changes
4. **Ensure tests pass** by running `dotnet test`
5. **Update documentation** if needed
6. **Write a clear commit message** following conventional commits
7. **Submit a pull request** with a clear description

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Aspire)

### Local Development

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/PoJoker.git
cd PoJoker

# Start the application with Aspire
dotnet run --project src/PoJoker.AppHost

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Coding Standards

This project follows specific architectural patterns and standards:

### .NET Standards

- **Target**: .NET 10 and C# 14
- **Package Management**: Central Package Management (CPM) via `Directory.Packages.props`
- **AOT Compatibility**: Enable `<IsAotCompatible>true</IsAotCompatible>`
- **Warnings as Errors**: Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

### Code Style

- Use **Primary Constructors** where applicable
- Use **Collection Expressions** (`[...]` syntax)
- Use the **field keyword** in properties
- Avoid reflection-based mappers; use **Mapperly** for DTO conversions
- Follow **Vertical Slice Architecture** (VSA) patterns

### Architecture

- **Feature Folders**: Keep endpoints, DTOs, and business logic together
- **Result Pattern**: Use the `ErrorOr` library for error handling
- **No Hardcoded Ports**: Rely on Aspire's service discovery

### Testing

- Write unit tests in `tests/Po.Joker.Tests.Unit`
- Write E2E tests in `tests/Po.Joker.Tests.E2E`
- Maintain or improve code coverage
- Follow existing test patterns

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add new joke category filter
fix: resolve punchline prediction timeout
docs: update deployment instructions
test: add unit tests for leaderboard service
refactor: simplify joke analysis logic
```

## Project Structure

```
PoJoker/
├── src/
│   ├── PoJoker.AppHost/        # Aspire orchestrator
│   ├── PoJoker.ServiceDefaults/ # Shared service defaults
│   ├── Po.Joker/               # Main Blazor application
│   ├── Po.Joker.Client/        # Client-side Blazor WASM
│   └── Po.Joker.Shared/        # DTOs and contracts
├── tests/
│   ├── Po.Joker.Tests.Unit/    # Unit tests
│   └── Po.Joker.Tests.E2E/     # E2E tests
├── infra/                       # Bicep IaC templates
└── docs/                        # Documentation
```

## Questions?

Feel free to open an issue with your question, or reach out to the maintainers.

## License

By contributing to Po.Joker, you agree that your contributions will be licensed under the [MIT License](LICENSE).
