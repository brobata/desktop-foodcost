# Desktop Food Cost

A professional cross-platform recipe costing and menu management application for restaurants, built with Avalonia UI and .NET 8.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.0-8B5CF6?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0MCIgaGVpZ2h0PSI0MCIgdmlld0JveD0iMCAwIDQwIDQwIj48cGF0aCBmaWxsPSIjZmZmIiBkPSJNMjAgMEwwIDIwaDEwTDIwIDEwbDEwIDEwaDEwTDIwIDB6Ii8+PC9zdmc+)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview

Desktop Food Cost helps restaurant owners, chefs, and kitchen managers calculate accurate food costs, manage recipes, and make data-driven menu pricing decisions. The application runs entirely locally with multi-location support, making it ideal for managing multiple restaurants or kitchen operations.

## Key Features

### Recipe & Menu Management
- **Recipe Builder**: Create recipes with ingredients, sub-recipes, portions, and step-by-step instructions
- **Entree Costing**: Build complete menu items from recipes and ingredients with automatic cost rollup
- **Photo Gallery**: Attach photos to recipes and entrees for visual reference
- **Version Control**: Track recipe changes over time with full version history

### Cost Analysis
- **Real-Time Costing**: Automatic cost calculation as you build recipes
- **Price History**: Track ingredient price changes over time
- **Cost Trends**: Visualize cost fluctuations with trend analysis charts
- **Profit Margins**: Set target margins and get pricing recommendations

### Multi-Location Support
- **Location Management**: Create and manage multiple restaurant locations
- **Location Switching**: Quick switch between locations from the header
- **Isolated Data**: Each location maintains its own ingredients, recipes, and pricing
- **Bulk Operations**: Import/export data between locations

### Data Management
- **Excel Import**: Bulk import ingredients from Excel spreadsheets
- **Excel Export**: Export recipes, entrees, and cost reports
- **Backup & Restore**: Automatic and manual database backups
- **Recycle Bin**: Recover accidentally deleted items

### Additional Features
- **Nutritional Data**: USDA FoodData Central integration for automatic nutritional info
- **Allergen Tracking**: Comprehensive allergen management and warnings
- **Unit Conversions**: Intelligent unit conversion system (weight, volume, each)
- **Shopping Lists**: Generate shopping lists from recipes and entrees
- **Portion Calculator**: Scale recipes up or down with automatic recalculation

## Screenshots

*Screenshots coming soon*

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 |
| UI | Avalonia UI 11.x (cross-platform XAML) |
| Database | SQLite with Entity Framework Core |
| Architecture | MVVM with CommunityToolkit.Mvvm |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Image Processing | SkiaSharp |

## Project Structure

```
dfc/
├── dfc.core/           # Core business logic, services, and interfaces
│   ├── Constants/      # Application constants
│   ├── Enums/          # Enumeration types
│   ├── Helpers/        # Utility classes
│   ├── Interfaces/     # Service interfaces
│   ├── Models/         # Domain models
│   └── Services/       # Business logic services
│
├── dfc.data/           # Data access layer
│   ├── LocalDatabase/  # SQLite DbContext and configuration
│   ├── Migrations/     # EF Core migrations
│   ├── Repositories/   # Repository implementations
│   └── Services/       # Data-specific services
│
├── dfc.desktop/        # Avalonia desktop application
│   ├── Assets/         # Images, icons, and resources
│   ├── Controls/       # Custom UI controls
│   ├── Converters/     # XAML value converters
│   ├── Models/         # View-specific models
│   ├── Services/       # Desktop-specific services
│   ├── ViewModels/     # MVVM ViewModels
│   └── Views/          # AXAML views and windows
│
├── dfc.tests/          # Unit and integration tests
└── installer/          # Inno Setup installer scripts
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, JetBrains Rider, or VS Code with C# extension

### Build

```bash
# Clone the repository
git clone https://github.com/brobata/desktop-foodcost.git
cd desktop-foodcost

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Run

```bash
dotnet run --project dfc.desktop
```

### Publish

```bash
# Windows (self-contained)
dotnet publish dfc.desktop -c Release -r win-x64 --self-contained -o ./publish/win-x64

# macOS (self-contained)
dotnet publish dfc.desktop -c Release -r osx-x64 --self-contained -o ./publish/osx-x64

# Linux (self-contained)
dotnet publish dfc.desktop -c Release -r linux-x64 --self-contained -o ./publish/linux-x64
```

## Configuration

### Optional: USDA API Key

For automatic nutritional data lookup, obtain a free API key from [USDA FoodData Central](https://fdc.nal.usda.gov/api-key-signup.html).

Set the environment variable:
```bash
export USDA_API_KEY=your_api_key_here
```

Or add it to your `.env` file in the application directory.

## Database

Desktop Food Cost uses SQLite for local data storage. The database is created automatically on first run at:

- **Windows**: `%APPDATA%\Desktop Food Cost\dfc.db`
- **macOS**: `~/Library/Application Support/Desktop Food Cost/dfc.db`
- **Linux**: `~/.local/share/Desktop Food Cost/dfc.db`

### Backups

Automatic backups are created daily in the `Backups` subdirectory. You can also create manual backups from Settings > Backup & Restore.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Roadmap

- [ ] Cloud sync option (Supabase backend)
- [ ] Mobile companion app
- [ ] Menu engineering analytics
- [ ] Vendor price comparison
- [ ] Inventory management integration
- [ ] Multi-language support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Brobata** - [brobata.com](https://brobata.com)

## Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform .NET UI framework
- [USDA FoodData Central](https://fdc.nal.usda.gov/) - Nutritional data API
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit
