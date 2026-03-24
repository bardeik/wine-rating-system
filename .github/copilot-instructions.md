# Wine Rating System - AI Coding Instructions

## Project Overview
A Norwegian wine judging system built on .NET 10 with Blazor Server UI. Judges can rate wines on Appearance, Nose, and Taste metrics. The system uses MongoDB for persistence and ASP.NET Core Identity for authentication and role-based access.



## Architecture

### Backend: ASP.NET Core on .NET 10
- **Primary project**: `WineApp/src/WineApp/`
- **Entry point**: `Program.cs` using modern minimal hosting model with `WebApplication.CreateBuilder`
- **No REST API**: The `Controllers/` folder is empty — Blazor Server uses direct service injection; no HTTP API layer exists
- **Models**: POCOs with nullable reference types in `Models/` — includes domain models, enums, and view models
- **Data layer**: Repository pattern using MongoDB via `WineMongoDbContext`
  - Interfaces in `Data/`: `IWineRepository`, `IWineRatingRepository`, `IWineProducerRepository`, `IEventRepository`, `IWineResultRepository`, `IPaymentRepository`, `IFlightRepository`
  - Each repository holds an `IMongoCollection<T>` sourced from `WineMongoDbContext`
  - **Database**: MongoDB (default: `mongodb://localhost:27017`, database name: `wineapp`)
  - All repository methods are `async Task<T>`
  - Uses modern C# patterns: file-scoped namespaces, target-typed new(), expression-bodied members

### Service Layer
Business logic and cross-cutting concerns live in `Services/`. **Blazor pages inject services — never repositories directly.**

**Facade services** (aggregate multiple repositories for pages):
- `IWineCatalogService` / `WineCatalogService` — wraps `IWineRepository` + `IWineProducerRepository`; used by most pages for wine and producer operations
- `IWineEventService` / `WineEventService` — wraps `IEventRepository`; event CRUD and active-event lookup
- `IWineJudgingService` / `WineJudgingService` — wraps `IWineRatingRepository` + `IWineResultRepository`; rating and result operations

**Domain services**:
- `IReportService` / `ReportService` — aggregates wine ratings into `WineReportRow` view models for the Reports page
- `IClassificationService` / `ClassificationService` — medal/classification logic; reads thresholds from the `Event` passed to each call via `GetThreshold(classification, eventConfig)`; never uses hardcoded values
- `IScoreAggregationService` / `ScoreAggregationService` — recalculates event results
- `IWineNumberService` / `WineNumberService` — assigns sequential wine numbers after payment
- `ITrophyService` / `TrophyService` — determines trophy winners (best Norwegian, best Nordic)
- `IOutlierDetectionService` / `OutlierDetectionService` — detects statistical outliers in judge scores
- `IWineValidationService` / `WineValidationService` — validates wine registration business rules
- `IFlightService` / `FlightService` — manages judge flights (groupings of wines per session)
- `IExportService` / `ExportService` — data export functionality
- `IPdfService` / `PdfService` — PDF generation
- `CurrentUserState` — scoped service holding resolved identity for the current Blazor Server circuit; call `await EnsureInitializedAsync()` in `OnInitializedAsync`

### Frontend: Blazor Server
- **Location**: `WineApp/src/WineApp/Pages/` and `Shared/`
- **Entry**: `Pages/_Host.cshtml` → `App.razor` → Blazor components
- **Routing**: `App.razor` wraps everything in `<CascadingAuthenticationState>` and uses `<AuthorizeRouteView>` with a `<RedirectToLogin />` fallback

**Pages**:
- `Wines.razor` — wine listing/management (Admin: all; WineProducer: own)
- `WineRatings.razor` — rating entry/display (Admin: all; Judge: own)
- `WineProducers.razor` — producer management (Admin: full CRUD + linked accounts; WineProducer: own profile)
- `Judges.razor` — Admin-only judge management (add/remove Judge role via `UserManager`)
- `Reports.razor` — aggregated wine score report using `IReportService`
- `Events.razor` — event management (Admin only); form includes medal thresholds with default-value hints and gate values with range constraints
- `EventDetails.razor` — event overview, flight and result detail; thresholds tab shows active medal thresholds and gate values
- `FlightManagement.razor` — manage judge flights via `IFlightService`
- `JudgeRating.razor` — tablet-optimised blind rating UI for judges; auto-saves; score-range labels and gate-value warnings are dynamically read from the active event's gate values (`activeEvent.AppearanceGateValue`, `NoseGateValue`, `TasteGateValue`)
- `ResultsReport.razor` — per-event results with recalculation
- `TrophyReports.razor` — trophy winners; Admin can recalculate
- `OutlierManagement.razor` — detect and manage outlier judge scores
- `PublicResults.razor` — public-facing results page (no auth required)
- `PaymentManagement.razor` — Admin confirms producer payments and assigns wine numbers
- `PaymentReceipt.razor` — receipt view for producers
- `WineRegistration.razor` — producer wine registration with grape blend editor

**Shared components**:
- `LoadingContainer.razor` — wraps page content; renders spinner when `IsLoading=true`, error alert when `Error` is non-empty, otherwise renders `ChildContent`
- `StatusAlert.razor` — dismissable status alert; parameters: `Message`, `CssClass` (default `"alert-info"`), `OnDismiss`
- `MainLayout.razor`, `NavMenu.razor`, `LoginDisplay.razor`, `RedirectToLogin.razor`, `RedirectToHome.razor`

**Page conventions**:
- Every data-loading page has `private bool isLoading = true` and `private string? loadError` fields
- Page markup is wrapped in `<LoadingContainer IsLoading="@isLoading" Error="@loadError">...</LoadingContainer>`
- `OnInitializedAsync` always uses try/catch/finally to set `isLoading = false`
- Status messages use `private string statusMessage = string.Empty` and `private string statusClass = "alert-info"` with `<StatusAlert Message="@statusMessage" CssClass="@statusClass" OnDismiss="() => statusMessage = string.Empty" />`
- `StateHasChanged()` is always called as `await InvokeAsync(StateHasChanged)` for thread safety

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
- **Test project**: `WineApp/tests/WineApp.Tests/` — xUnit 2.x, Moq 4.x, Shouldly 4.x; added to the solution

## Key Conventions

### Dependency Injection
All repositories and services are registered as **Scoped** so each Blazor Server circuit gets its own instance, preventing shared mutable state across concurrent users. `WineMongoDbContext` stays `AddSingleton`. `TimeProvider.System` is registered as `AddSingleton` so the production clock is injectable and replaceable in tests.

```csharp
// MongoDB context — Singleton (thread-safe, holds collection references only)
builder.Services.AddSingleton<WineMongoDbContext>();

// Clock abstraction — lets tests substitute a frozen clock
builder.Services.AddSingleton(TimeProvider.System);

// Repositories — Scoped
builder.Services.AddScoped<IWineRepository, WineRepository>();
builder.Services.AddScoped<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddScoped<IWineProducerRepository, WineProducerRepository>();
// ... all other repositories

// Services — Scoped
builder.Services.AddScoped<IWineCatalogService, WineCatalogService>();
builder.Services.AddScoped<IWineEventService, WineEventService>();
builder.Services.AddScoped<IWineJudgingService, WineJudgingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<CurrentUserState>();
// ... all other services
```

Identity registered with:
```csharp
builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(...)
    .AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(connectionString, dbName)
    .AddDefaultTokenProviders();
```

Blazor pages inject services (not repositories):
```razor
@inject IWineCatalogService WineCatalog
@inject IWineEventService EventSvc
@inject CurrentUserState CurrentUser
```

### MongoDB Models
Models use BSON attributes for MongoDB serialisation:
```csharp
[MongoDB.Bson.Serialization.Attributes.BsonId]
[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
public string WineId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
```

### Wine Domain Model
- **Wine**: has `WineGroup` (A1/A2/B/C/D enum), `WineClass` (Unge/Eldre), `WineCategory` (Hvitvin/Rosevin/etc.) enums; linked to `WineProducer` by `WineProducerId`; has `GrapeBlend Dictionary<string, decimal>`, `IsPaid`, `WineNumber`
- **WineRating**: judges rate wines on `Appearance` (0-3.0), `Nose` (0-4.0), `Taste` (0-13.0) as `decimal`; stores `JudgeId` (judge's `DisplayName`) and `WineId`
- **WineProducer**: has a `UserId` (string) linking to the corresponding `ApplicationUser`
- **WineResult**: computed result per wine per event (aggregated from ratings)
- **Event**: has `EventId`, `Name`, `IsActive`; only one active event at a time. Also holds all configurable thresholds and gate values:
  - `GoldThreshold`, `SilverThreshold`, `BronzeThreshold`, `SpecialMeritThreshold` (defaults 17.0 / 15.5 / 14.0 / 12.0)
  - `AdjustedGoldThreshold`, `AdjustedSilverThreshold`, `AdjustedBronzeThreshold`, `AdjustedSpecialMeritThreshold` (alternative set, defaults 15.0 / 14.0 / 13.0 / 11.5)
  - `UseAdjustedThresholds` (bool) — when true, `ClassificationService` uses the adjusted set
  - `AppearanceGateValue`, `NoseGateValue`, `TasteGateValue` (defaults 1.8 / 1.8 / 5.8; minimum 0.1 enforced by `[Range]`)
  - `OutlierThreshold` (default 4.0)
  - All threshold fields carry `[Range]` and `[DisplayName]` attributes
- **Flight**: groups wines for a judge session; managed by `IFlightService`

### View Models (in `Models/`)
- `WineReportRow` — flattened row for the Reports page (avg scores, totals)
- `JudgeFormModel` — form model for adding judges (validated with DataAnnotations)
- `WineProducerFormModel` — form model for adding wine producers with login account
- `JudgePattern` — outlier detection result per judge

### Async Pattern
All repository and service methods are `async Task<T>`. Pages always `await` them:
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        wines = await WineCatalog.GetAllWinesAsync();
    }
    catch (Exception ex)
    {
        loadError = ex.Message;
    }
    finally
    {
        isLoading = false;
    }
}
```

### TimeProvider Pattern
Never use `DateTime.Now` or `DateTime.UtcNow` directly inside services. Inject `TimeProvider` instead so the clock can be frozen in tests:

```csharp
// Service constructor
public class MyService : IMyService
{
    private readonly TimeProvider _timeProvider;

    public MyService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void DoSomething()
    {
        var now   = _timeProvider.GetLocalNow();     // replaces DateTime.Now
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime; // replaces DateTime.UtcNow
    }
}
```

In tests, use `FrozenTimeProvider` from `WineApp.Tests` to pin time:
```csharp
var clock = FrozenTimeProvider.At(2026, 6, 1);
var sut = new MyService(clock);  // now is deterministically 2026-06-01
```

## Development Workflow

### Prerequisites
- MongoDB running locally on `mongodb://localhost:27017` (or update `appsettings.json`)

### Building & Running
```bash
cd WineApp/src/WineApp
dotnet restore
dotnet build WineApp.csproj
dotnet run
# Default URL: https://localhost:5001 or http://localhost:5000
```
On first run, `Program.cs` seeds all roles, user accounts, sample wine producers, wines, and ratings into MongoDB if the collections are empty.

### Running Tests
```bash
cd WineApp
dotnet test tests/WineApp.Tests/WineApp.Tests.csproj
```
- Tests use **xUnit**, **Moq** (for repository mocks), and **Shouldly** (for assertions)
- `GlobalUsings.cs` in the test project provides global usings for all three libraries plus the main project namespaces
- `FrozenTimeProvider` (in `WineApp.Tests`) freezes the clock for deterministic time-dependent tests
- No MongoDB or Blazor infrastructure is needed — all services are tested with in-memory mocks

### Blazor Development
- Components are hot-reloadable in development mode
- Use `@code` blocks for C# logic within `.razor` files
- CSS isolation supported via `.razor.css` files
- Bootstrap CSS framework included for styling
- `_Imports.razor` includes `@using WineApp.Models`, `@using WineApp.Data`, `@using WineApp.Services`, `@using WineApp.Shared`

## Project Structure Notes
- **Multiple legacy projects**: `WineRatingApp`, `WebAppCore`, `skeleton-typescript-aspnetcore` appear to be earlier iterations — ignore these
- **Focus on WineApp**: primary working implementation is in `WineApp/src/WineApp/`
- **Test project**: `WineApp/tests/WineApp.Tests/` — unit tests for service layer (no Blazor/MongoDB needed)
- **No REST API**: `Controllers/` is intentionally empty; all UI interactions go through Blazor service injection

## Common Tasks

### Adding a New Entity
1. Create model in `Models/` (POCO with BsonId/BsonRepresentation attributes, file-scoped namespace)
2. Add `IMongoCollection<T>` property to `WineMongoDbContext`
3. Create repository interface in `Data/IEntityRepository.cs` with async CRUD methods
4. Implement repository in `Data/EntityRepository.cs`
5. Register in `Program.cs`: `builder.Services.AddScoped<IEntityRepository, EntityRepository>()`
6. Add methods to an appropriate facade service (`IWineCatalogService`, `IWineEventService`, or `IWineJudgingService`), or create a new service
7. Create Blazor page in `Pages/EntityName.razor` — inject the service, not the repository
8. Add route in `NavMenu.razor` inside the correct `<AuthorizeView>` block

### Adding a New Blazor Page
1. Create `.razor` file in `Pages/`
2. Add `@page "/route"` directive
3. Add `@attribute [Authorize]` (or role-specific variant)
4. Inject required services with `@inject`
5. Add `isLoading`/`loadError` fields and wrap markup in `<LoadingContainer>`
6. Add `statusMessage`/`statusClass` fields and `<StatusAlert>` for user feedback
7. Use `@code` block with `async Task OnInitializedAsync()` wrapped in try/catch/finally
8. Reference in `NavMenu.razor` inside the appropriate `<AuthorizeView>` block

### Adding a New Service
1. Create interface `Services/IMyService.cs` with async method signatures
2. Implement in `Services/MyService.cs`; inject required repositories or other services via constructor
   - If the service uses the current date/time, inject `TimeProvider` instead of calling `DateTime.Now`/`UtcNow` directly
3. Register in `Program.cs`: `builder.Services.AddScoped<IMyService, MyService>()`
4. Inject into pages via `@inject IMyService MyService`
5. Add a corresponding `Services/MyServiceTests.cs` in `WineApp/tests/WineApp.Tests/Services/`; mock repositories with Moq, assert with Shouldly, and use `FrozenTimeProvider` for time-sensitive paths

## Technology Stack
- **.NET SDK**: 10.0
- **ASP.NET Core**: 10.0
- **Blazor Server**: for interactive UI
- **MongoDB**: data persistence via `MongoDB.Driver` 3.6.0
- **ASP.NET Core Identity + MongoDB**: authentication/authorisation via `AspNetCore.Identity.MongoDbCore` 7.0.0
- **Bootstrap**: for styling (referenced in `wwwroot/css/`)
- **No JavaScript framework**: pure Blazor/C#
- **No REST API**: Blazor Server uses direct DI — no controllers, no HttpClient
- **Unit tests**: xUnit 2.x + Moq 4.x + Shouldly 4.x in `WineApp/tests/WineApp.Tests/`

