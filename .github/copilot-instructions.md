# Wine Rating System - AI Coding Instructions

## Project Overview
A Norwegian wine judging system upgraded to .NET 10 with Blazor Server UI. Judges can rate wines on Visuality, Nose, and Taste metrics.

## Architecture

### Backend: ASP.NET Core on .NET 10
- **Primary project**: `WineApp/src/WineApp/`
- **Entry point**: `Program.cs` using modern minimal hosting model with WebApplication.CreateBuilder
- **API**: RESTful controllers under `Controllers/` with `[Route("api/[controller]")]` attribute
- **Models**: Simple POCOs with nullable reference types in `Models/` - includes `Wine`, `WineRating`, `WineProducer`, `TodoItem`
- **Data layer**: Repository pattern WITHOUT Entity Framework - all repositories use in-memory `List<T>`
  - Interfaces: `IWineRatingRepository`, `IWineRepository`, etc. in `Data/`
  - Implementations: Seeded with hardcoded sample data (e.g., judges named "Hans", "Petter", "Frans", "Ola")
  - **No database**: Data doesn't persist across application restarts
  - Uses modern C# patterns: file-scoped namespaces, target-typed new(), expression-bodied members

### Frontend: Blazor Server
- **Location**: `WineApp/src/WineApp/Pages/` and `Shared/`
- **Entry**: `Pages/_Host.cshtml` → `App.razor` → Blazor components
- **Routing**: Configured in `App.razor` with routes to /wines, /wineproducers, /wineratings
- **Components**: 
  - `Wines.razor` - Wine listing and management
  - `WineRatings.razor` - Rating entry and display
  - `WineProducers.razor` - Producer management
  - `MainLayout.razor` - Main application layout
  - `NavMenu.razor` - Navigation sidebar
- **Data access**: Direct repository injection via DI (no HTTP calls needed)
- **Forms**: Uses Blazor's `EditForm`, `InputText`, `InputNumber`, `InputSelect` components

### Project Configuration
- **SDK**: .NET 10.0 (see `global.json`)
- **Project file**: Modern SDK-style `.csproj` with ImplicitUsings and Nullable enabled
- **No legacy files**: project.json, xproj, and Startup.cs removed in favor of modern patterns

## Key Conventions

### Dependency Injection
Singleton services registered in `Program.cs`:
```csharp
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
```
- Blazor components inject via `@inject IWineRepository WineRepository`
- Controllers inject via constructor parameters

### JSON Serialization
Uses System.Text.Json with camelCase naming policy:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

### API Patterns
- GET all: `GET /api/wineratings` → `IEnumerable<T>`
- GET by ID: `GET /api/wineratings/{id}` → `T` (nullable)
- POST: `POST /api/wineratings` with JSON body → `201 Created` with Location header
- DELETE: `DELETE /api/wineratings/{id}` → removes from in-memory list

### Wine Domain Model
- **Wine**: Has `WineGroup` (A/B/C/D enum), `WineClass` (Unge/Eldre), `WineCategory` (Hvitvin/Rosevin/etc.) enums, linked to `WineProducer` by ID
- **WineRating**: Judges rate wines on `Visuality`, `Nose`, `Taste` (integer scores), stores `JudgeId` (string) and `WineId`
- All models use file-scoped namespaces and nullable reference types with string.Empty defaults

## Development Workflow

### Building & Running
```bash
# Navigate to project
cd WineApp/src/WineApp

# Restore .NET dependencies
dotnet restore

# Build application
dotnet build WineApp.csproj

# Run application
dotnet run
# Default URL: https://localhost:5001 or http://localhost:5000
```

### Blazor Development
- Components are hot-reloadable in development mode
- Use `@code` blocks for C# logic within .razor files
- CSS isolation supported via `.razor.css` files
- Bootstrap CSS framework included for styling

## Project Structure Notes
- **Multiple legacy projects**: `WineRatingApp`, `WebAppCore`, `skeleton-typescript-aspnetcore` appear to be earlier iterations - ignore these
- **Focus on WineApp**: Primary working implementation is in `WineApp/src/WineApp/`
- **Old frontend removed**: Previous Aurelia/TypeScript SPA (wwwroot/src, jspm_packages) can be deleted

## Common Tasks

### Adding a New Entity
1. Create model in `Models/` (POCO with nullable reference types, file-scoped namespace)
2. Create repository interface in `Data/IEntityRepository.cs` with CRUD methods
3. Implement repository in `Data/EntityRepository.cs` with `List<T>` and sample data
4. Register in `Program.cs`: `builder.Services.AddSingleton<IEntityRepository, EntityRepository>()`
5. Create controller in `Controllers/` (optional, for API access)
6. Create Blazor page in `Pages/EntityName.razor` with @inject directive for repository
7. Add route in `NavMenu.razor` if navigation item is needed

### Adding a New Blazor Component
1. Create `.razor` file in `Pages/` or `Shared/`
2. Add `@page "/route"` directive for routable pages
3. Inject services with `@inject ServiceType VariableName`
4. Use `@code` block for C# logic
5. Reference in `NavMenu.razor` if needed for navigation

## Technology Stack
- **.NET SDK**: 10.0.102
- **ASP.NET Core**: 10.0
- **Blazor Server**: For interactive UI
- **Bootstrap**: For styling (referenced in wwwroot/css/)
- **No database**: In-memory data only
- **No JavaScript framework**: Pure Blazor/C#

