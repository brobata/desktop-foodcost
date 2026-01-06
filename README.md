# Freecost

A cross-platform recipe cost management application built with Avalonia UI and .NET 8.

## Features

- **Recipe Management**: Create and manage recipes with ingredients, portions, and photos
- **Cost Tracking**: Track ingredient costs and calculate recipe pricing
- **Nutritional Data**: USDA integration for automatic nutritional information
- **Allergen Tracking**: Manage allergen information for ingredients and recipes
- **Cloud Sync**: Supabase-powered sync across devices
- **Cross-Platform**: Windows desktop application (macOS/Linux support planned)

## Technology Stack

- **Framework**: .NET 8
- **UI**: Avalonia UI (cross-platform XAML)
- **Database**: SQLite (local), Supabase (cloud)
- **Architecture**: MVVM with dependency injection

## Project Structure

```
FreecostAvalonia/
├── Freecost.Core/        # Business logic and services
├── Freecost.Data/        # Data models and database access
├── Freecost.Desktop/     # Avalonia UI application
├── Freecost.Tests/       # Unit tests
└── installer/            # Inno Setup installer scripts
```

## Building

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022, Rider, or VS Code

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project Freecost.Desktop
```

### Publish

```bash
dotnet publish Freecost.Desktop -c Release -r win-x64 --self-contained
```

## Configuration

Freecost requires the following environment variables:

| Variable | Description |
|----------|-------------|
| `SUPABASE_URL` | Your Supabase project URL |
| `SUPABASE_ANON_KEY` | Your Supabase anon/public key |
| `USDA_API_KEY` | USDA FoodData Central API key (optional) |

See `installer/SETUP_CREDENTIALS.md` for detailed setup instructions.

## License

Proprietary - See [LICENSE.txt](LICENSE.txt)

## Author

Brobata - https://brobata.com
