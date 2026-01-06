# Contributing to Desktop Food Cost

First off, thank you for considering contributing to Desktop Food Cost! It's people like you that make this tool better for everyone.

## Code of Conduct

By participating in this project, you are expected to uphold our values of respect, inclusivity, and constructive collaboration.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Describe the behavior you observed and what you expected**
- **Include screenshots if possible**
- **Include your environment details** (OS, .NET version, app version)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description of the suggested enhancement**
- **Explain why this enhancement would be useful**
- **Include mockups or examples if applicable**

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding style** used throughout the project
3. **Write clear commit messages**
4. **Add tests** for new functionality when applicable
5. **Update documentation** as needed
6. **Ensure all tests pass** before submitting

## Development Setup

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code with C# extension
- Git

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/desktop-foodcost.git
cd desktop-foodcost

# Add upstream remote
git remote add upstream https://github.com/brobata/desktop-foodcost.git

# Create a branch for your work
git checkout -b feature/your-feature-name

# Restore dependencies and build
dotnet restore
dotnet build

# Run the application
dotnet run --project dfc.desktop

# Run tests
dotnet test
```

## Project Structure

```
dfc/
├── dfc.core/        # Business logic - no UI dependencies
├── dfc.data/        # EF Core, repositories, migrations
├── dfc.desktop/     # Avalonia UI application
└── dfc.tests/       # Unit and integration tests
```

### Architecture Guidelines

- **MVVM Pattern**: ViewModels in `dfc.desktop/ViewModels/`, Views in `dfc.desktop/Views/`
- **Dependency Injection**: Register services in `App.axaml.cs`
- **Repository Pattern**: Data access through repositories in `dfc.data/Repositories/`
- **Service Layer**: Business logic in `dfc.core/Services/`

## Coding Standards

### C# Style

- Use meaningful names for variables, methods, and classes
- Prefer `var` when the type is obvious
- Use async/await for I/O operations
- Follow [Microsoft C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### AXAML Style

- Use consistent indentation (4 spaces)
- Group related properties together
- Use meaningful `x:Name` values when needed
- Prefer bindings over code-behind when possible

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit first line to 72 characters
- Reference issues and PRs when relevant

Example:
```
Add ingredient import from CSV files

- Parse CSV with configurable column mapping
- Validate ingredient data before import
- Show progress and error summary

Closes #123
```

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test dfc.tests
```

### Writing Tests

- Place tests in `dfc.tests` project
- Mirror the source structure (e.g., `Services/RecipeServiceTests.cs`)
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Include both positive and negative test cases

## Database Migrations

When making database changes:

```bash
# Add a new migration
cd dfc.data
dotnet ef migrations add YourMigrationName

# Apply migrations (happens automatically on app start)
dotnet ef database update
```

## Questions?

Feel free to open an issue with your question or reach out to the maintainers.

Thank you for contributing!
