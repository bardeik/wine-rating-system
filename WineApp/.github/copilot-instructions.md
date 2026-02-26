# Wine Rating System - AI Coding Instructions

## Project Overview
A comprehensive Norwegian wine judging system (Norsk Vinskue) built on .NET 10 with Blazor Server UI. Judges rate wines on Appearance (A: 0–3), Nose (B: 0–4), and Taste (C: 0–13) with decimal precision. The system uses MongoDB for persistence and ASP.NET Core Identity for authentication and role-based access.

**Current Status:** All 6 Phases Complete — Production Ready

---

## Architecture

### Backend: ASP.NET Core on .NET 10
- **Primary project**: `WineApp/src/WineApp/`
- **Entry point**: `Program.cs` using modern minimal hosting model with `WebApplication.CreateBuilder`
- **API**: RESTful controllers under `Controllers/` with `[Route("api/[controller]")]` attribute
- **Models**: POCOs with nullable reference types in `Models/` — includes `Wine`, `WineRating`, `WineProducer`, `ApplicationUser`, `Event`, `WineResult`, `Payment`, `Flight`
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
- **Data access**: direct repository injection via DI (no HTTP calls from Blazor)

---

## Layout & Navigation

### MainLayout.razor
- `.sidebar` div holds `<NavMenu>` and gets CSS class `open` when `NavMenu.IsMenuOpen == true`
- Dark backdrop overlay rendered when menu is open; clicking it calls `NavMenu.CloseMenu()`
- Hamburger button (`oi-menu`) in the top-row — **visible only on screens <1100px**
- Uses `@ref="NavMenu"` to call `NavMenu.ToggleMenu()` and `NavMenu.CloseMenu()`

### NavMenu.razor
- Implements `IDisposable` — unsubscribes from `NavigationManager.LocationChanged` in `Dispose()`
- `IsMenuOpen` public property; `ToggleMenu()` and `CloseMenu()` public async methods
- `OnMenuToggle` EventCallback notifies MainLayout to call `StateHasChanged()`
- Auto-closes on navigation via `NavigationManager.LocationChanged`
- All nav links always show icon + text (no icon-only collapsed mode)
- `nav-brand` header at top showing "Wine App"
- **Do NOT add a hamburger button inside NavMenu** — it lives in MainLayout's top-row

### Responsive Behaviour (MainLayout.razor.css)
- **≥1100px (desktop)**: `.sidebar` is `position: sticky`, 250px wide, always visible. Hamburger hidden.
- **<1100px (tablet/mobile)**: `.sidebar` is `position: fixed`, slides in from left as overlay. `.sidebar.open` = visible. `main` uses full 100% width via `flex-direction: column` on `.page`.

---

## Key Blazor Pages

### Public Access
- `PublicResults.razor` — Trophy winners, medal statistics, top 20 results (no auth required)

### Wine Producer Pages
- `WineRegistration.razor` — Registration with interactive grape blend editor (must sum to 100%), real-time validation, producer dashboard. Only unpaid wines editable.
- `PaymentReceipt.razor` — Payment info, bank details (Norwegian + International), wine list with status, wine numbers after payment

### Judge Pages
- `JudgeRating.razor` — Tablet-optimized rating interface
  - Flight selection, large touch-friendly inputs, auto-save (2s debounce)
  - Progress tracking, gate value warnings
  - "Tidligere vurdering" shown as compact badges inside the wine-card
  - iPad Air optimized via `@media (min-width: 768px) and (max-width: 1024px)`
- `WineRatings.razor` — Classic rating list (legacy)

### Admin Pages
- `Events.razor` — Event CRUD, wine number assignment, result calculation, archival
- `FlightManagement.razor` — Organize wines into flights (simple/auto), CSV export
- `PaymentManagement.razor` — Payment tracking, bulk confirmation, auto wine number assignment
- `TrophyReports.razor` — Trophy winner reports with CSV/PDF export
- `ResultsReport.razor` — Advanced results with filtering, search, sorting, CSV export
- `OutlierManagement.razor` — Outlier detection, view ratings per wine, mark as resolved
- `Wines.razor` — Wine listing/management
- `WineProducers.razor` — Producer management
- `Judges.razor` — Judge role management
- `Reports.razor` — Aggregated score reports (legacy)
- `EventDetails.razor` — Event detail view with trophy winners

### Shared Components
- `LoginDisplay.razor` — Current user name and logout button
- `RedirectToLogin.razor` — Redirects to `/Account/Login`
- `RedirectToHome.razor` — Redirects to `/?accessDenied=true`

---

## Authentication & Authorisation
- **Provider**: ASP.NET Core Identity backed by MongoDB via `AspNetCore.Identity.MongoDbCore`
- **User model**: `ApplicationUser : MongoIdentityUser<Guid>` (adds `DisplayName`), `[CollectionName("Users")]`
- **Role model**: `MongoIdentityRole<Guid>`
- **Roles**: `Admin`, `Judge`, `Viewer`, `WineProducer`
- **Cookie**: login path `/Account/Login`, 8-hour sliding expiration
- **Access Denied**: redirect to `/?accessDenied=true` — `Index.razor` shows a dismissible alert
- **Page guards**: `@attribute [Authorize]` or `@attribute [Authorize(Roles = "...")]`
- **Seeded in `DatabaseSeeder.cs`** (first run, if collections empty):

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

---

## Project Configuration
- **SDK**: .NET 10.0
- **NuGet packages**: `AspNetCore.Identity.MongoDbCore` 7.0.0, `MongoDB.Driver` 3.6.0, `QuestPDF` 2026.2.2
- **Project file**: modern SDK-style `.csproj` with `ImplicitUsings` and `Nullable` enabled
- **Connection string**: `appsettings.json` → `ConnectionStrings:MongoDB` and `MongoDbSettings:DatabaseName`

---

## Business Logic Services (all Singletons in Program.cs)

| Service | Purpose |
|---|---|
| `IScoreAggregationService` | Panel averages, total score, defect/gate/outlier detection |
| `IClassificationService` | Gull/Sølv/Bronse/Særlig/Akseptabel/IkkeGodkjent. Adjusted thresholds when no Gold |
| `IWineNumberService` | Sequential wine numbers for paid wines, ordered by WineCategory enum |
| `ITrophyService` | Trophy winners with tie-break via `HighestSingleScore`; `RequiresLottery` if still tied |
| `IOutlierDetectionService` | Spread > `Event.OutlierThreshold` (default 4.0) |
| `IWineValidationService` | Grape blend 100%, A2 not Norwegian, IsVinbonde only A1, etc. |
| `IFlightService` | Simple/auto flight organization (stored in-memory) |
| `IExportService` | CSV export with UTF-8 BOM for Excel |
| `IPdfService` | QuestPDF-based trophy report PDF |

---

## Key Conventions

### Dependency Injection
```csharp
// Repositories
builder.Services.AddSingleton<WineMongoDbContext>();
builder.Services.AddSingleton<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddSingleton<IWineRepository, WineRepository>();
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IWineResultRepository, WineResultRepository>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

// Business Logic Services
builder.Services.AddSingleton<IClassificationService, ClassificationService>();
builder.Services.AddSingleton<IScoreAggregationService, ScoreAggregationService>();
builder.Services.AddSingleton<IWineNumberService, WineNumberService>();
builder.Services.AddSingleton<ITrophyService, TrophyService>();
builder.Services.AddSingleton<IOutlierDetectionService, OutlierDetectionService>();
builder.Services.AddSingleton<IWineValidationService, WineValidationService>();
builder.Services.AddSingleton<IFlightService, FlightService>();
builder.Services.AddSingleton<IExportService, ExportService>();
builder.Services.AddSingleton<IPdfService, PdfService>();
```

Identity registered with:
```csharp
builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(...)
    .AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(connectionString, dbName)
    .AddDefaultTokenProviders();
```

- Blazor components inject via `@inject IServiceName ServiceName`
- Add `@using WineApp.Services` when injecting business logic services

### MongoDB Models
```csharp
[MongoDB.Bson.Serialization.Attributes.BsonId]
[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
public string WineId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
```

### JSON Serialization
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

### Download Endpoints (Minimal API in Program.cs)
- `GET /api/download/results/{eventId}` — CSV results (auth required)
- `GET /api/download/trophies/{eventId}` — CSV trophies (auth required)
- `GET /api/download/event/{eventId}` — CSV archive (Admin only)
- `GET /api/download/flights/{eventId}` — CSV flights (Admin only)
- `GET /api/download/trophy-pdf/{eventId}` — PDF trophy report (auth required)

Usage: `Navigation.NavigateTo($"/api/download/results/{eventId}", forceLoad: true)`

---

## Wine Domain Model

### WineRating Scoring (decimal, 0.1 step)
- `Appearance` (0.0–3.0) gate ≥1.8
- `Nose` (0.0–4.0) gate ≥1.8
- `Taste` (0.0–13.0) gate ≥5.8

### WineGroup Enum
- **A1**: Godkjente sorter friland (Norge)
- **A2**: Nordiske gjesteviner
- **B**: Godkjente sorter veksthus
- **C**: Prøvesorter friland
- **D**: Prøvesorter veksthus

### WineCategory Enum (wine number ordering)
Hvitvin → Rosevin → Dessertvin → Rødvin → Musserende vin → Hetvin

### WineResult Key Fields
- `AverageAppearance`, `AverageNose`, `AverageTaste`, `TotalScore`
- `Classification`, `IsDefective`, `IsOutlier`, `Spread`, `MeetsGateValues`
- `HighestSingleScore`, `RequiresLottery`, `NumberOfRatings`

### Event Classification Thresholds (defaults)
- Gold ≥17.0, Silver ≥15.5, Bronze ≥14.0, SpecialMerit ≥12.0
- Adjusted (when no Gold): 15.0 / 14.0 / 13.0 / 11.5
- `OutlierThreshold` default 4.0

### Critical Field Name Notes
- Use `AverageAppearance` NOT `PanelAverageAppearance`
- Use `NumberOfRatings` NOT `JudgeCount`
- Use `IsDefective` NOT `HasDefect`
- Use `AppearanceGateValue`, `NoseGateValue`, `TasteGateValue` on `Event`

---

## Common Tasks

### Adding a New Entity
1. Model in `Models/` (BsonId/BsonRepresentation, file-scoped namespace)
2. `IMongoCollection<T>` in `WineMongoDbContext` (`Data/WineAppDbContext.cs`)
3. Repository interface + implementation in `Data/`
4. Register as Singleton in `Program.cs`
5. Blazor page in `Pages/` with `[Authorize]`
6. Nav link in `NavMenu.razor` inside correct `<AuthorizeView>`

### Adding a Navigation Link
```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="my-route">
        <span class="oi oi-ICON" aria-hidden="true"></span> Label
    </NavLink>
</div>
```
Place inside the appropriate `<AuthorizeView Roles="...">` block in `NavMenu.razor`.

### Adding a New Service
1. `Services/IServiceName.cs` + `Services/ServiceName.cs`
2. `builder.Services.AddSingleton<IServiceName, ServiceName>()` in `Program.cs`
3. `@inject IServiceName ServiceName` + `@using WineApp.Services` in Blazor

### Adding a New Blazor Page
1. Create `.razor` in `Pages/`
2. `@page "/route"` + `@attribute [Authorize(Roles = "...")]`
3. Inject services + implement UI
4. Add nav link in `NavMenu.razor`

---

## Development Workflow

### Prerequisites
- MongoDB running locally on `mongodb://localhost:27017`

### Build & Run
```bash
cd WineApp/src/WineApp
dotnet restore
dotnet build WineApp.csproj
dotnet run
```
On first run, `DatabaseSeeder.cs` seeds roles, users, and sample data.

### Blazor Best Practices
- Always call `StateHasChanged()` after async operations that modify UI state
- Use `@bind:after` for real-time updates in .NET 10
- CSS isolation via `.razor.css` files
- Bootstrap CSS framework + Open Iconic (`oi`) icon font

---

## Project Structure Notes
- **Multiple legacy projects**: `WineRatingApp`, `WebAppCore`, `skeleton-typescript-aspnetcore` — ignore these
- **Focus on**: `WineApp/src/WineApp/`

---

## Technology Stack
- **.NET 10 / ASP.NET Core 10 / Blazor Server**
- **MongoDB** via `MongoDB.Driver` 3.6.0
- **ASP.NET Core Identity** via `AspNetCore.Identity.MongoDbCore` 7.0.0
- **QuestPDF** 2026.2.2 (PDF generation)
- **Bootstrap** (styling), **Open Iconic** (`oi`) icon font

---

*Last Updated: June 2025 — All 6 Phases Complete*
