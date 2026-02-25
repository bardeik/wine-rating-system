# Wine Rating System - AI Coding Instructions

## Project Overview
A Norwegian wine judging system built on .NET 10 with Blazor Server UI. Judges can rate wines on Visuality, Nose, and Taste metrics. The system uses MongoDB for persistence and ASP.NET Core Identity for authentication and role-based access.

## Architecture

### Backend: ASP.NET Core on .NET 10
- **Primary project**: `WineApp/src/WineApp/`
- **Entry point**: `Program.cs` using modern minimal hosting model with `WebApplication.CreateBuilder`
- **API**: RESTful controllers under `Controllers/` with `[Route("api/[controller]")]` attribute
- **Models**: POCOs with nullable reference types in `Models/` - includes `Wine`, `WineRating`, `WineProducer`, `ApplicationUser`
- **Data layer**: Repository pattern using MongoDB via `WineMongoDbContext`
  - Interfaces: `IWineRatingRepository`, `IWineRepository`, `IWineProducerRepository` in `Data/`
  - Implementations: each repository holds an `IMongoCollection<T>` sourced from `WineMongoDbContext`
  - **Database**: MongoDB (default: `mongodb://localhost:27017`, database name: `wineapp`)
  - Uses modern C# patterns: file-scoped namespaces, target-typed new(), expression-bodied members

### Frontend: Blazor Server
- **Location**: `WineApp/src/WineApp/Pages/` and `Shared/`
- **Entry**: `Pages/_Host.cshtml` → `App.razor` → Blazor components
- **Routing**: `App.razor` wraps everything in `<CascadingAuthenticationState>` and uses `<AuthorizeRouteView>` with a `<RedirectToLogin />` fallback for unauthenticated users
- **Components**:
  - `Wines.razor` - wine listing/management (Admin sees all; WineProducer sees own)
  - `WineRatings.razor` - rating entry/display (Admin sees all; Judge sees own)
  - `WineProducers.razor` - producer management (Admin: full CRUD + linked user accounts; WineProducer: own profile edit)
  - `Judges.razor` - judge management (Admin only: list, add, remove Judge role via `UserManager`)
  - `Reports.razor` - aggregated wine score report
  - `MainLayout.razor` - main layout with `<LoginDisplay />` in the top bar
  - `NavMenu.razor` - navigation sidebar with `<AuthorizeView>` guards per role
  - `LoginDisplay.razor` - shows current user name and logout button
  - `RedirectToLogin.razor` - redirects unauthenticated users to `/Account/Login`
- **Data access**: direct repository injection via DI (no HTTP calls from Blazor)
- **Forms**: uses Blazor's `EditForm`, `InputText`, `InputNumber`, `InputSelect` components

### Authentication & Authorisation
- **Provider**: ASP.NET Core Identity backed by MongoDB via `AspNetCore.Identity.MongoDbCore`
- **User model**: `ApplicationUser : MongoIdentityUser<Guid>` (adds `DisplayName` property), decorated with `[CollectionName("Users")]`
- **Role model**: `MongoIdentityRole<Guid>`
- **Roles**: `Admin`, `Judge`, `Viewer`, `WineProducer`
- **Cookie**: login path `/Account/Login`, access-denied path `/Account/AccessDenied`, 8-hour sliding expiration
- **Page guards**: `@attribute [Authorize]` or `@attribute [Authorize(Roles = "...")]` on Razor components
- **Seeded accounts** (created in `Program.cs` on first run if MongoDB collections are empty):

  | Email | Password | Roles |
  |---|---|---|
  | admin@wineapp.com | Admin123! | Admin, Viewer |
  | viewer@wineapp.com | Viewer123! | Viewer |
  | hans@wineapp.com | Judge123! | Judge, Viewer |
  | petter@wineapp.com | Judge123! | Judge, Viewer |
  | frans@wineapp.com | Judge123! | Judge, Viewer |
  | ola@wineapp.com | Judge123! | Judge, Viewer |
  | oslo@wineapp.com | Producer123! | WineProducer, Viewer |
  | grimstad@wineapp.com | Producer123! | WineProducer, Viewer |
  | techwine@wineapp.com | Producer123! | WineProducer, Viewer |

### Project Configuration
- **SDK**: .NET 10.0
- **NuGet packages**: `AspNetCore.Identity.MongoDbCore` 7.0.0, `MongoDB.Driver` 3.6.0
- **Project file**: modern SDK-style `.csproj` with `ImplicitUsings` and `Nullable` enabled
- **Connection string**: `appsettings.json` → `ConnectionStrings:MongoDB` and `MongoDbSettings:DatabaseName`

## Key Conventions

### Dependency Injection
Services registered in `Program.cs`:
```csharp
builder.Services.AddSingleton<WineMongoDbContext>();
builder.Services.AddSingleton<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddSingleton<IWineRepository, WineRepository>();
```
Identity registered with:
```csharp
builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(...)
    .AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(connectionString, dbName)
    .AddDefaultTokenProviders();
```
- Blazor components inject repositories via `@inject IWineRepository WineRepository`
- Auth-aware Blazor pages also inject `UserManager<ApplicationUser>` and `AuthenticationStateProvider`
- Controllers inject via constructor parameters

### MongoDB Models
Models use BSON attributes for MongoDB serialisation:
```csharp
[MongoDB.Bson.Serialization.Attributes.BsonId]
[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
public string WineId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
```

### JSON Serialization
Uses `System.Text.Json` with camelCase naming policy:
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
- DELETE: `DELETE /api/wineratings/{id}` → removes document from MongoDB collection

### Wine Domain Model
- **Wine**: has `WineGroup` (A/B/C/D enum), `WineClass` (Unge/Eldre), `WineCategory` (Hvitvin/Rosevin/etc.) enums, linked to `WineProducer` by `WineProducerId` (ObjectId string)
- **WineRating**: judges rate wines on `Visuality`, `Nose`, `Taste` (integers 0-10); stores `JudgeId` (the judge's `DisplayName`) and `WineId`
- **WineProducer**: has a `UserId` (string) linking to the corresponding `ApplicationUser` identity account
- All models use file-scoped namespaces and nullable reference types with `string.Empty` defaults

## Development Workflow

### Prerequisites
- MongoDB running locally on `mongodb://localhost:27017` (or update `appsettings.json`)

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
On first run `Program.cs` seeds all roles, user accounts, sample wine producers, wines, and ratings into MongoDB if the collections are empty.

### Blazor Development
- Components are hot-reloadable in development mode
- Use `@code` blocks for C# logic within `.razor` files
- CSS isolation supported via `.razor.css` files
- Bootstrap CSS framework included for styling

## Project Structure Notes
- **Multiple legacy projects**: `WineRatingApp`, `WebAppCore`, `skeleton-typescript-aspnetcore` appear to be earlier iterations - ignore these
- **Focus on WineApp**: primary working implementation is in `WineApp/src/WineApp/`

## Common Tasks

### Adding a New Entity
1. Create model in `Models/` (POCO with BsonId/BsonRepresentation attributes, file-scoped namespace)
2. Add `IMongoCollection<T>` property to `WineMongoDbContext`
3. Create repository interface in `Data/IEntityRepository.cs` with CRUD methods
4. Implement repository in `Data/EntityRepository.cs` using the MongoDB collection
5. Register in `Program.cs`: `builder.Services.AddSingleton<IEntityRepository, EntityRepository>()`
6. Create controller in `Controllers/` (optional, for API access)
7. Create Blazor page in `Pages/EntityName.razor` with `@inject` and the appropriate `[Authorize]` attribute
8. Add route in `NavMenu.razor` inside the correct `<AuthorizeView>` block

### Adding a New Blazor Component
1. Create `.razor` file in `Pages/` or `Shared/`
2. Add `@page "/route"` directive for routable pages
3. Add `@attribute [Authorize]` (or a role-specific variant) to protect the page
4. Inject services with `@inject ServiceType VariableName`
5. Use `@code` block for C# logic
6. Reference in `NavMenu.razor` inside the appropriate `<AuthorizeView>` block

## Technology Stack
- **.NET SDK**: 10.0
- **ASP.NET Core**: 10.0
- **Blazor Server**: for interactive UI
- **MongoDB**: data persistence via `MongoDB.Driver` 3.6.0
- **ASP.NET Core Identity + MongoDB**: authentication/authorisation via `AspNetCore.Identity.MongoDbCore` 7.0.0
- **Bootstrap**: for styling (referenced in `wwwroot/css/`)
- **No JavaScript framework**: pure Blazor/C#

