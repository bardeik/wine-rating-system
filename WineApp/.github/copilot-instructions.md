# Wine Rating System - AI Coding Instructions

## Project Overview
A Norwegian wine judging system (Norsk Vinskue) built on .NET 10 with Blazor Server UI. Judges rate wines on Appearance (A: 0-3), Nose (B: 0-4), and Taste (C: 0-13) with decimal precision. The system uses MongoDB for persistence and ASP.NET Core Identity for authentication and role-based access.

## Architecture

### Backend: ASP.NET Core on .NET 10
- **Primary project**: `WineApp/src/WineApp/`
- **Entry point**: `Program.cs` using modern minimal hosting model with `WebApplication.CreateBuilder`
- **API**: RESTful controllers under `Controllers/` with `[Route("api/[controller]")]` attribute
- **Models**: POCOs with nullable reference types in `Models/` - includes `Wine`, `WineRating`, `WineProducer`, `ApplicationUser`, `Event`, `WineResult`, `Payment`
- **Data layer**: Repository pattern using MongoDB via `WineMongoDbContext`
  - Interfaces: `IWineRatingRepository`, `IWineRepository`, `IWineProducerRepository`, `IEventRepository`, `IWineResultRepository`, `IPaymentRepository` in `Data/`
  - Implementations: each repository holds an `IMongoCollection<T>` sourced from `WineMongoDbContext`
  - **Database**: MongoDB (default: `mongodb://localhost:27017`, database name: `wineapp`)
  - **DB Context file**: `Data/WineAppDbContext.cs` (contains class `WineMongoDbContext`)
  - Uses modern C# patterns: file-scoped namespaces, target-typed new(), expression-bodied members

### Frontend: Blazor Server
- **Location**: `WineApp/src/WineApp/Pages/` and `Shared/`
- **Entry**: `Pages/_Host.cshtml` → `App.razor` → Blazor components
- **Routing**: `App.razor` wraps everything in `<CascadingAuthenticationState>` and uses `<AuthorizeRouteView>` with a `<RedirectToLogin />` fallback for unauthenticated users
- **Components**:
  - `Index.razor` - home page with welcome message and navigation cards; displays access denied alert when redirected from unauthorized page via `?accessDenied=true` query parameter
  - `Wines.razor` - wine listing/management (Admin sees all; WineProducer sees own)
  - `WineRatings.razor` - rating entry/display with decimal inputs (Admin sees all; Judge sees own)
  - `WineProducers.razor` - producer management (Admin: full CRUD + linked user accounts; WineProducer: own profile edit)
  - `Judges.razor` - judge management (Admin only: list, add, remove Judge role via `UserManager`)
  - `Reports.razor` - aggregated wine score report
  - `MainLayout.razor` - main layout with `<LoginDisplay />` in the top bar
  - `NavMenu.razor` - navigation sidebar with `<AuthorizeView>` guards per role
  - `LoginDisplay.razor` - shows current user name and logout button
  - `RedirectToLogin.razor` - redirects unauthenticated users to `/Account/Login` with return URL
  - `RedirectToHome.razor` - redirects users who lack authorization to home page (`/?accessDenied=true`)
- **Data access**: direct repository injection via DI (no HTTP calls from Blazor)
- **Forms**: uses Blazor's `EditForm`, `InputText`, `InputNumber`, `InputSelect` components
- **Utilities**: `Data/GlobalLists.cs` contains helper methods for select lists (legacy, may be unused in current Blazor implementation)

### Authentication & Authorisation
- **Provider**: ASP.NET Core Identity backed by MongoDB via `AspNetCore.Identity.MongoDbCore`
- **User model**: `ApplicationUser : MongoIdentityUser<Guid>` (adds `DisplayName` property), decorated with `[CollectionName("Users")]`
- **Role model**: `MongoIdentityRole<Guid>`
- **Roles**: `Admin`, `Judge`, `Viewer`, `WineProducer`
- **Cookie**: login path `/Account/Login`, 8-hour sliding expiration
- **Access Denied**: users without proper authorization are redirected to home page (`/`) with `?accessDenied=true` query parameter, where an alert message is displayed
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
- **WineGroup**: enum with values `A1` (Norwegian approved grapes, outdoor), `A2` (Nordic guest wines), `B` (approved grapes, greenhouse), `C` (trial grapes, outdoor), `D` (trial grapes, greenhouse)
- **Wine**: has `WineNumber` (unique assigned number), `WineGroup`, `WineClass` (Unge/Eldre), `WineCategory` (Hvitvin/Rosevin/Dessertvin/Rodvin/Mousserendevin/Hetvin), `Vintage` (year), `AlcoholPercentage` (decimal), `GrapeBlend` (Dictionary<string, decimal>), `IsVinbonde` (≥100 vinstokker), `Country`, `IsPaid`, `EventId`; linked to `WineProducer` by `WineProducerId` (ObjectId string)
- **WineRating**: judges rate wines on `Appearance` (A: 0.0-3.0), `Nose` (B: 0.0-4.0), `Taste` (C: 0.0-13.0) with **decimal precision (one decimal place)**; includes `Comment` field; stores `JudgeId` (the judge's `DisplayName`), `WineId`, and `RatingDate`
- **WineProducer**: has `MemberNumber`, `Phone`, `UserId` (string) linking to the corresponding `ApplicationUser` identity account, plus contact details
- **Event**: represents a competition year with registration windows, payment info (IBAN/BIC, Org.nr), medal thresholds (gold ≥17.0, silver ≥15.5, bronze ≥14.0, special merit ≥12.0), gate values (appearance ≥1.8, nose ≥1.8, taste ≥5.8), outlier threshold (4.0), and adjusted thresholds for when no gold is awarded
- **WineResult**: aggregated scores per wine with panel averages for A/B/C, total score (sum of averages), classification (Gull/Sølv/Bronse/Særlig/Akseptabel/IkkeGodkjent), defect/outlier flags, spread calculation, and tie-break data
- **Payment**: tracks payment per producer/event with wine IDs, amount, payment status, receipt status
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
- **Focus on WineApp**: primary working implementation is in `WineApp/src/WineApp/`
- **Legacy files**: `Data/GlobalLists.cs` contains older select list helpers that may not be actively used in current Blazor components

## Common Tasks

### Adding a New Entity
1. Create model in `Models/` (POCO with BsonId/BsonRepresentation attributes, file-scoped namespace)
2. Add `IMongoCollection<T>` property to `WineMongoDbContext` in `Data/WineAppDbContext.cs`
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

### Handling Access Denied Scenarios
- Unauthenticated users: `RedirectToLogin.razor` redirects to `/Account/Login` with return URL
- Authenticated but unauthorized users: `RedirectToHome.razor` redirects to `/?accessDenied=true`, where `Index.razor` displays a dismissible alert

## Technology Stack
- **.NET SDK**: 10.0
- **ASP.NET Core**: 10.0
- **Blazor Server**: for interactive UI
- **MongoDB**: data persistence via `MongoDB.Driver` 3.6.0
- **ASP.NET Core Identity + MongoDB**: authentication/authorisation via `AspNetCore.Identity.MongoDbCore` 7.0.0
- **Bootstrap**: for styling (referenced in `wwwroot/css/`)
- **No JavaScript framework**: pure Blazor/C#
