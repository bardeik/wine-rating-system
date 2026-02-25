# Wine Rating System - AI Coding Instructions

## Project Overview
A comprehensive Norwegian wine judging system (Norsk Vinskue) built on .NET 10 with Blazor Server UI. Judges rate wines on Appearance (A: 0-3), Nose (B: 0-4), and Taste (C: 0-13) with decimal precision. The system uses MongoDB for persistence and ASP.NET Core Identity for authentication and role-based access.

**Current Status:** ✅ **All 6 Phases Complete** - Production Ready

---

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
- **JavaScript**: `wwwroot/js/site.js` for file downloads and print functionality
- **Routing**: `App.razor` wraps everything in `<CascadingAuthenticationState>` and uses `<AuthorizeRouteView>` with a `<RedirectToLogin />` fallback for unauthenticated users

### Key Blazor Components

#### **Public Access**
- `PublicResults.razor` - Public results display (no auth required)
  - Trophy winners showcase
  - Medal statistics
  - Top 20 results table
  - Modern gradient design

#### **Wine Producer Pages**
- `WineRegistration.razor` - Enhanced wine registration with:
  - Interactive grape blend editor (must sum to 100%)
  - Real-time validation
  - Event context display
  - Producer dashboard (list of own wines)
  - View/Edit/Delete workflows (only unpaid wines editable)
- `PaymentReceipt.razor` - Payment information and status
  - Bank details (Norwegian + International)
  - Wine list with payment status
  - Wine numbers after payment
  - Important dates and deadlines

#### **Judge Pages**
- `JudgeRating.razor` - Tablet-optimized rating interface
  - Flight selection
  - Large touch-friendly inputs (2rem font)
  - Auto-save (2-second debounce)
  - Progress tracking
  - Gate value warnings
  - Rating history
  - Keyboard shortcuts (Tab, Enter)
- `WineRatings.razor` - Classic rating list/entry (legacy)

#### **Admin Pages**
- `Events.razor` - Event management with CRUD operations
  - Wine number assignment (batch)
  - Result calculation (batch)
  - Event archival (CSV export)
  - Activation control
- `FlightManagement.razor` - Organize wines into flights
  - Simple organization (sequential)
  - Auto organization (by category/group)
  - Flight list export (CSV)
  - Delete flights
- `PaymentManagement.razor` - Payment tracking
  - Producer payment overview
  - Bulk payment confirmation
  - Individual payment confirmation
  - Automatic wine number assignment on payment
  - Filter by payment status
- `TrophyReports.razor` - Trophy winner reports
  - Årets Vinbonde (A1 + Vinbonde status)
  - Beste norske vin (A1/B/C/D)
  - Beste nordiske vin (A1/A2)
  - CSV export
  - Print functionality
- `ResultsReport.razor` - Advanced results list
  - Filter by group/class/category/classification
  - Search by wine/producer name
  - Sortable columns
  - CSV export
  - Statistics dashboard
- `OutlierManagement.razor` - Outlier detection and management
  - Identify wines with spread > threshold
  - View detailed ratings per wine
  - Mark as resolved
  - Judge pattern analysis (placeholder)
- `Wines.razor` - Wine listing/management
- `WineProducers.razor` - Producer management
- `Judges.razor` - Judge role management
- `Reports.razor` - Aggregated score reports (legacy)
- `EventDetails.razor` - Event detail view with trophy winners

#### **Shared Components**
- `MainLayout.razor` - Main layout with `<LoginDisplay />`
- `NavMenu.razor` - Navigation sidebar with `<AuthorizeView>` guards per role
- `LoginDisplay.razor` - Current user name and logout
- `RedirectToLogin.razor` - Redirects to `/Account/Login`
- `RedirectToHome.razor` - Redirects to `/?accessDenied=true`

### Authentication & Authorisation
- **Provider**: ASP.NET Core Identity backed by MongoDB via `AspNetCore.Identity.MongoDbCore`
- **User model**: `ApplicationUser : MongoIdentityUser<Guid>` (adds `DisplayName` property), decorated with `[CollectionName("Users")]`
- **Role model**: `MongoIdentityRole<Guid>`
- **Roles**: `Admin`, `Judge`, `Viewer`, `WineProducer`
- **Cookie**: login path `/Account/Login`, 8-hour sliding expiration
- **Access Denied**: users without proper authorization are redirected to home page (`/`) with `?accessDenied=true` query parameter, where an alert message is displayed
- **Page guards**: `@attribute [Authorize]` or `@attribute [Authorize(Roles = "...")]` on Razor components
- **Seeded accounts** (created in `DatabaseSeeder.cs` on first run if MongoDB collections are empty):

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
- **NuGet packages**: 
  - `AspNetCore.Identity.MongoDbCore` 7.0.0
  - `MongoDB.Driver` 3.6.0
  - `QuestPDF` 2026.2.2
- **Project file**: modern SDK-style `.csproj` with `ImplicitUsings` and `Nullable` enabled
- **Connection string**: `appsettings.json` → `ConnectionStrings:MongoDB` and `MongoDbSettings:DatabaseName`

---

## Business Logic Services

All services registered as Singletons in `Program.cs`:

### 1. **IScoreAggregationService / ScoreAggregationService**
**Purpose:** Calculate panel averages and aggregate scores

**Key Methods:**
- `CalculateWineResult(string wineId)` - Single wine calculation
- `RecalculateEventResultsAsync(string eventId)` - Batch calculation for all wines in event

**Calculations:**
- Panel averages: A (Appearance), B (Nose), C (Taste)
- Total score: Sum of panel averages
- Defect detection: Any dimension = 0 OR taste ≤ 1
- Gate value validation: A ≥1.8, B ≥1.8, C ≥5.8
- Spread calculation: Max rating - Min rating across all judges
- Outlier flagging: Spread > event.OutlierThreshold (default 4.0)
- Highest single score tracking (for tie-breaks)

**Output:** `WineResult` object stored in `WineResultRepository`

### 2. **IClassificationService / ClassificationService**
**Purpose:** Classify wines based on total score and thresholds

**Classifications:**
- **Gull** (Gold): ≥17.0 (or ≥15.0 adjusted)
- **Sølv** (Silver): ≥15.5 (or ≥14.0 adjusted)
- **Bronse** (Bronze): ≥14.0 (or ≥13.0 adjusted)
- **Særlig** (Special Merit): ≥12.0 (or ≥11.5 adjusted)
- **Akseptabel** (Acceptable): ≥0 but below Special Merit
- **IkkeGodkjent** (Not Approved): Has defect OR doesn't meet gate values

**Rules:**
- Defect → automatic "IkkeGodkjent"
- Gate value failure → automatic "IkkeGodkjent"
- Uses adjusted thresholds when no Gold is awarded
- Event can toggle `UseAdjustedThresholds`

### 3. **IWineNumberService / WineNumberService**
**Purpose:** Assign sequential wine numbers for blind tasting

**Key Method:**
- `AssignWineNumbersAsync(string eventId)` - Batch assignment

**Logic:**
- Only paid wines get numbers
- Sequential numbering starting from 1
- Ordered by category (enum order):
  1. Hvitvin (White)
  2. Rosevin (Rosé)
  3. Dessertvin (Dessert)
  4. Rodvin (Red)
  5. Mousserendevin (Sparkling)
  6. Hetvin (Fortified)
- Numbers are immutable once assigned

### 4. **ITrophyService / TrophyService**
**Purpose:** Determine trophy winners with tie-break logic

**Trophies:**
1. **Årets Vinbonde** - Highest score in Group A1 with `IsVinbonde = true`
2. **Vinskuets beste norske vin** - Highest score in groups A1, B, C, D
3. **Vinskuets beste nordiske vin** - Highest score in groups A1, A2

**Tie-Break Logic:**
1. Compare total scores
2. If tied → compare `HighestSingleScore` (from any judge)
3. If still tied → set `RequiresLottery = true`

**Eligibility:**
- Wine must have medal classification (Gull, Sølv, or Bronse)
- Wine must be paid and have a wine number
- Wine must meet trophy-specific group requirements

**Returns:** `(Wine? wine, WineResult? result)` tuple

### 5. **IOutlierDetectionService / OutlierDetectionService**
**Purpose:** Detect scoring outliers and analyze judge patterns

**Key Methods:**
- `GetOutliers(string eventId)` - Returns wines with spread > threshold
- `AnalyzeJudgePatterns(string eventId)` - Returns Dictionary<JudgeId, List<Issues>>

**Outlier Detection:**
- Spread = Max rating - Min rating across all judges
- Outlier if Spread > `Event.OutlierThreshold` (default 4.0)
- Stored as `IsOutlier = true` in WineResult

**Judge Pattern Analysis:**
- Identifies judges with consistently low/high scores
- Detects low variance (lack of discrimination)
- Flags high defect rate (>30%)

### 6. **IWineValidationService / WineValidationService**
**Purpose:** Validate wine registration data

**Validations:**
- Grape blend sums to 100% (±0.01% tolerance)
- All required fields populated
- A2 wines cannot be from Norway (Nordic guests only)
- `IsVinbonde` only valid for Group A1
- Vintage year within acceptable range
- Alcohol percentage within valid range

**Returns:** `(bool isValid, List<string> errors)`

### 7. **IFlightService / FlightService**
**Purpose:** Organize wines into flights for tasting sessions

**Key Methods:**
- `OrganizeFlights(eventId, winesPerFlight)` - Simple sequential organization
- `AutoOrganizeFlights(eventId)` - Group by category then group
- `GetFlightsForEvent(eventId)` - Retrieve all flights
- `GetWinesInFlight(flightId)` - Get wines in specific flight

**Flight Model:**
```csharp
public class Flight
{
    public string FlightId { get; set; }
    public string EventId { get; set; }
    public string FlightName { get; set; }
    public int FlightNumber { get; set; }
    public List<string> WineIds { get; set; }
    public WineCategory? Category { get; set; }
    public WineGroup? Group { get; set; }
}
```

**Storage:** In-memory (can be moved to MongoDB if needed)

### 8. **IExportService / ExportService**
**Purpose:** Export data to CSV format

**Key Methods:**
- `ExportResultsToCSV(results, wines, producers)` - Complete results export
- `ExportTrophiesToCSV(trophies, producers)` - Trophy winners export
- `ExportEventData(event, wines, ratings, results, producers)` - Event archival
- `ExportFlightList(flights, wines)` - Flight list for printing
- `GetCSVBytes(csvContent)` - UTF-8 BOM encoded bytes for Excel

**Features:**
- Proper CSV escaping (quotes, commas, newlines)
- UTF-8 with BOM for Excel compatibility
- Comprehensive data export for archival

### 9. **IPdfService / PdfService**
**Purpose:** Generate PDF documents for reports

**Key Methods:**
- `GenerateTrophyReport(event, trophies, producers)` - Professional PDF report

**Features:**
- QuestPDF-based generation
- Professional layout with colors and styling
- Trophy winner details with all metadata
- Lottery warnings for tied scores
- A4 page format with proper margins

---

## Key Conventions

### Dependency Injection
Services registered in `Program.cs`:
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

- Blazor components inject repositories and services via `@inject`
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

### JavaScript Interop

**Note:** No JavaScript is needed in this application. All file downloads (CSV, PDF) are handled server-side via minimal API endpoints. See Download Endpoints section below.

---

## Download Endpoints (Minimal API)

File downloads are handled server-side via minimal API endpoints in `Program.cs`:

**CSV Endpoints:**
- `GET /api/download/results/{eventId}` - Export complete results (requires authentication)
- `GET /api/download/trophies/{eventId}` - Export trophy winners as CSV (requires authentication)
- `GET /api/download/event/{eventId}` - Export event archive (requires Admin role)
- `GET /api/download/flights/{eventId}` - Export flight list (requires Admin role)

**PDF Endpoints:**
- `GET /api/download/trophy-pdf/{eventId}` - Export trophy report as PDF (requires authentication)

**Usage in Blazor:**
```csharp
@inject NavigationManager Navigation

private void ExportData()
{
    Navigation.NavigateTo($"/api/download/results/{eventId}", forceLoad: true);
}
```

**Benefits:**
- No JavaScript required
- Browser handles file download natively
- Direct file streaming (more efficient)
- Proper authorization via ASP.NET Core policies
- Professional PDF generation with QuestPDF

---

## Wine Domain Model

### WineGroup Enum
- **A1**: Godkjente sorter friland (Norge) - Norwegian approved grapes, outdoor
- **A2**: Nordiske gjesteviner - Nordic guest wines
- **B**: Godkjente sorter veksthus - Approved grapes, greenhouse
- **C**: Prøvesorter friland - Trial grapes, outdoor
- **D**: Prøvesorter veksthus - Trial grapes, greenhouse

### WineClass Enum
- **Unge**: Young wines
- **Eldre**: Older/aged wines

### WineCategory Enum
- **Hvitvin**: White wine
- **Rosevin**: Rosé wine
- **Dessertvin**: Dessert wine
- **Rodvin**: Red wine
- **Mousserendevin**: Sparkling wine
- **Hetvin**: Fortified wine

### Wine Model
**Key Properties:**
- `WineNumber` (int?, assigned after payment)
- `Name` (string) - Public wine name
- `RatingName` (string) - Secret name for blind tasting
- `Vintage` (int) - Year
- `AlcoholPercentage` (decimal)
- `Group` (WineGroup enum)
- `Class` (WineClass enum)
- `Category` (WineCategory enum)
- `Country` (string)
- `IsVinbonde` (bool) - ≥100 vinstokker, only valid for A1
- `GrapeBlend` (Dictionary<string, decimal>) - Must sum to 100%
- `WineProducerId` (string, ObjectId)
- `EventId` (string, ObjectId)
- `IsPaid` (bool)
- `SubmissionDate` (DateTime)

**Linked to:** `WineProducer` via `WineProducerId`

### WineRating Model
**Scoring:**
- `Appearance` (decimal 0.0-3.0) - Gate value: ≥1.8
- `Nose` (decimal 0.0-4.0) - Gate value: ≥1.8
- `Taste` (decimal 0.0-13.0) - Gate value: ≥5.8
- **Decimal precision:** One decimal place (0.1 step)
- `Comment` (string, optional)
- `JudgeId` (string) - Judge's DisplayName
- `WineId` (string, ObjectId)
- `RatingDate` (DateTime)

**Important:** Ratings can be updated via `IWineRatingRepository.UpdateWineRating()`

### WineResult Model
**Calculated Fields:**
- `AverageAppearance` (decimal) - Panel average for A
- `AverageNose` (decimal) - Panel average for B
- `AverageTaste` (decimal) - Panel average for C
- `TotalScore` (decimal) - Sum of averages
- `Classification` (string) - Gull/Sølv/Bronse/Særlig/Akseptabel/IkkeGodkjent
- `IsDefective` (bool) - Any dimension = 0 OR taste ≤ 1
- `IsOutlier` (bool) - Spread > threshold
- `Spread` (decimal) - Max - Min across judges
- `HighestSingleScore` (decimal) - For tie-breaks
- `HighestScoreJudgeId` (string?) - Judge who gave highest score
- `MeetsGateValues` (bool)
- `RequiresLottery` (bool) - Tied with another wine
- `NumberOfRatings` (int) - Count of judge ratings
- `CalculationDate` (DateTime)

### WineProducer Model
- `MemberNumber` (string)
- `WineyardName` (string)
- `ResponsibleProducerName` (string)
- `Email` (string)
- `Phone` (string)
- `UserId` (string, nullable) - Links to `ApplicationUser` for login

### Event Model
**Key Configuration:**
- `Name`, `Year`
- `RegistrationStartDate`, `RegistrationEndDate`
- `PaymentDeadline`, `DeliveryDeadline`
- `FeePerWine` (decimal)
- `BankName`, `AccountNumber`, `IBAN`, `BIC`, `OrganizationNumber`
- `DeliveryAddressNorway`, `ImporterInfoNordic`

**Thresholds:**
- `GoldThreshold` (default 17.0)
- `SilverThreshold` (default 15.5)
- `BronzeThreshold` (default 14.0)
- `SpecialMeritThreshold` (default 12.0)

**Adjusted Thresholds** (when no Gold awarded):
- `AdjustedGoldThreshold` (default 15.0)
- `AdjustedSilverThreshold` (default 14.0)
- `AdjustedBronzeThreshold` (default 13.0)
- `AdjustedSpecialMeritThreshold` (default 11.5)

**Gate Values:**
- `AppearanceGateValue` (default 1.8)
- `NoseGateValue` (default 1.8)
- `TasteGateValue` (default 5.8)

**Outlier:**
- `OutlierThreshold` (default 4.0)

**Flags:**
- `UseAdjustedThresholds` (bool)
- `IsActive` (bool) - Only one event can be active

### Payment Model
- `PaymentId`, `EventId`, `WineProducerId`
- `WineIds` (List<string>)
- `Amount` (decimal)
- `PaymentStatus` (string) - Pending/Paid
- `PaymentDate` (DateTime?)
- `ReceiptStatus` (string) - NotSent/Sent

---

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

On first run, `DatabaseSeeder.cs` seeds all roles, user accounts, sample wine producers, wines, and ratings into MongoDB if the collections are empty.

### Blazor Development
- Components are hot-reloadable in development mode
- Use `@code` blocks for C# logic within `.razor` files
- CSS isolation supported via `.razor.css` files
- Bootstrap CSS framework included for styling
- JavaScript in `wwwroot/js/site.js` for download/print

---

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

### Adding a New Service
1. Create interface `Services/IServiceName.cs`
2. Create implementation `Services/ServiceName.cs`
3. Register in `Program.cs`: `builder.Services.AddSingleton<IServiceName, ServiceName>()`
4. Inject in Blazor components or controllers via `@inject` or constructor

### Handling Access Denied Scenarios
- Unauthenticated users: `RedirectToLogin.razor` redirects to `/Account/Login` with return URL
- Authenticated but unauthorized users: `RedirectToHome.razor` redirects to `/?accessDenied=true`, where `Index.razor` displays a dismissible alert

---

## Technology Stack
- **.NET SDK**: 10.0
- **ASP.NET Core**: 10.0
- **Blazor Server**: for interactive UI
- **MongoDB**: data persistence via `MongoDB.Driver` 3.6.0
- **ASP.NET Core Identity + MongoDB**: authentication/authorisation via `AspNetCore.Identity.MongoDbCore` 7.0.0
- **Bootstrap**: for styling (referenced in `wwwroot/css/`)
- **JavaScript**: Minimal interop for downloads and printing
- **No JavaScript framework**: pure Blazor/C#

---

## Implementation Status

### ✅ Phase 1: Core Data Model (COMPLETE)
- All models defined with proper MongoDB attributes
- Repository pattern implemented for all entities
- Database seeding with sample data

### ✅ Phase 2: Business Logic (COMPLETE)
- 6 business logic services implemented
- Score aggregation with defect detection
- Wine classification with medal thresholds
- Trophy determination with tie-breaks
- Wine number assignment
- Outlier detection and judge analysis
- Wine registration validation

### ✅ Phase 3: Registration & Payment (COMPLETE)
- Enhanced wine registration with grape blend editor
- Real-time validation
- Payment management (admin)
- Payment receipt (producer)
- Automatic wine number assignment on payment
- Bulk payment operations

### ✅ Phase 4: Judge Experience (COMPLETE)
- Flight organization (simple and auto)
- Tablet-optimized rating UI
- Auto-save functionality
- Progress tracking
- Rating history
- Flight management (admin)

### ✅ Phase 5: Admin & Reports (COMPLETE)
- Trophy reports with winners
- Advanced results list with filtering
- Outlier management interface
- CSV export for all reports

### ✅ Phase 6: Enhancements (COMPLETE)
- CSV export service
- Event archival
- Public results display
- Print functionality
- File download via JavaScript interop

---

## Project Statistics

### Code Metrics
- **Total Files**: 70+ files
- **Models**: 8 (Wine, WineRating, WineProducer, ApplicationUser, Event, WineResult, Payment, Flight)
- **Repositories**: 6 interfaces + implementations
- **Services**: 8 interfaces + implementations
- **Blazor Pages**: 20+ (including admin, judge, producer, and public pages)
- **Total Lines of Code**: ~10,000+

### Navigation Structure
```
Public:
  - Home
  - Public Results

Producer:
  - Wine Registration
  - My Wines
  - Payment Receipt
  - Producer Profile

Judge:
  - Judge Rating (tablet UI)
  - My Ratings (classic)

Admin:
  - Events Management
  - Flight Management
  - Payment Management
  - Trophy Reports
  - Results Report
  - Outlier Management
  - Judges Management
  - Producers Management

Viewer:
  - Trophy Reports
  - Results Report
  - Reports (legacy)
```

---

## Key Features by Role

### Wine Producer
✅ Register wines with complete metadata  
✅ Interactive grape blend editor (must sum to 100%)  
✅ View payment information and bank details  
✅ Track payment status per wine  
✅ See wine numbers after payment  
✅ Edit/delete unpaid wines only  

### Judge
✅ Tablet-optimized rating interface  
✅ Select and rate wines by flight  
✅ Auto-save ratings (2-second debounce)  
✅ See gate value warnings in real-time  
✅ Track progress through flights  
✅ View previous ratings (rating history)  
✅ Keyboard shortcuts for efficiency  

### Admin
✅ Manage events (CRUD + activation)  
✅ Organize wines into flights  
✅ Confirm payments (individual or bulk)  
✅ Assign wine numbers automatically  
✅ Calculate results (batch)  
✅ View trophy winners  
✅ Export data (CSV): results, trophies, flights, event archives  
✅ Manage outliers (re-judging workflow)  
✅ Filter and search all data  
✅ Manage judges and producers  

### Viewer
✅ View trophy reports  
✅ View results lists  
✅ Access all reports (read-only)  

### Public (No Login)
✅ View published results  
✅ See trophy winners  
✅ Medal statistics  
✅ Top 20 wines  

---

## Important Notes

### Field Naming Conventions
- `WineResult` uses `AverageAppearance`, `AverageNose`, `AverageTaste` (NOT `PanelAverage*`)
- `WineResult` uses `NumberOfRatings` (NOT `JudgeCount`)
- `WineResult` uses `IsDefective` (NOT `HasDefect`)
- `Event` uses `AppearanceGateValue`, `NoseGateValue`, `TasteGateValue` (NOT `GateAppearance`, etc.)

### Repository Methods
- All repositories have standard CRUD: `GetAll`, `GetById`, `Add`, `Delete`
- `IWineRatingRepository` has `UpdateWineRating()` for editing ratings
- Use `MongoDB.Driver.ReplaceOne()` for updates

### Blazor Best Practices
- Always call `StateHasChanged()` after async operations that modify state
- Use `@bind:after` for real-time updates in Blazor .NET 10
- Inject services with `@inject` at the top of `.razor` files
- Use `@using WineApp.Services` when injecting services
- Add `@attribute [Authorize(Roles = "...")]` for role-based protection

### CSV Export
- Always use `ExportService` for CSV generation
- Files include UTF-8 BOM for Excel compatibility
- Download via **minimal API endpoints** and `NavigationManager.NavigateTo()`
- Proper escaping for quotes, commas, and newlines
- Server-side file streaming (no JavaScript needed for downloads)

### Flight Organization
- Flights stored in-memory (can be moved to MongoDB if needed)
- Auto-organize groups by Category → Group → WineNumber
- Only paid wines with wine numbers included in flights

---

## Future Enhancements (Not Implemented)

### Potential Additions
- PDF generation for diplomas/certificates
- Email notifications for producers
- Multi-language support (English)
- Swipe navigation for tablet UI
- Offline support for judges
- Payment API integration
- Bulk payment import (CSV)
- WCAG AA compliance audit

---

*Last Updated: December 2024*  
*Project Status: Production Ready*  
*All 6 Phases Complete*
