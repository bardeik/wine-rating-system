# Wine Rating System

A Norwegian wine judging and competition management system. The application manages the full lifecycle of a wine competition — from producer registration and wine submission, through blind judging by a panel, to medal classification, trophy awards, and public results publishing.

---

## Functionality

### Roles
The system has four roles, each with a distinct view of the application:

| Role | Description |
|---|---|
| **Admin** | Full access: manage events, producers, judges, payments, flights, results and reports |
| **Judge** | Rate wines assigned to their flight; view their own ratings |
| **WineProducer** | Register wines for an event; confirm payment; view receipts and results |
| **Viewer** | Read-only access to public results |

---

### Core Features

#### Wine Registration
Wine producers register their wines via a guided form. Each wine carries:
- Name, vintage, alcohol percentage and country of origin
- Grape blend (percentage per variety)
- Wine group (A1, A2, B, C, D), class (Unge/Eldre) and category (Hvitvin, Rosevin, Rødvin, etc.)
- Whether the producer qualifies as a *Vinbonde* (≥ 100 vines)

Business rules are enforced by `IWineValidationService`.

#### Payment & Wine Numbers
After registration, wines are not visible to judges until payment is confirmed. An Admin confirms each payment in **Payment Management**, after which `IWineNumberService` assigns a sequential wine number. Producers can view a payment receipt.

#### Events
An Admin creates and manages competitions (events). Only one event can be active at a time. All judging and results are scoped to the active event.

Each event carries its own fully configurable classification thresholds and gate values:

| Setting | Default | Description |
|---|---|---|
| Gold threshold | 17.0 | Minimum total score for Gull |
| Silver threshold | 15.5 | Minimum total score for Sølv |
| Bronze threshold | 14.0 | Minimum total score for Bronse |
| Special Merit threshold | 12.0 | Minimum total score for Særlig |
| Appearance gate value | 1.8 | Minimum avg appearance score (0.1–3.0) |
| Nose gate value | 1.8 | Minimum avg nose score (0.1–4.0) |
| Taste gate value | 5.8 | Minimum avg taste score (0.1–13.0) |
| Outlier threshold | 4.0 | Score spread above which a wine is flagged |

All values are validated with `[Range]` attributes; gate values require a minimum of 0.1. Default values are shown as help text in the Admin form.

#### Flight Management
Wines are distributed into judge flights by an Admin via **Flight Management**. Each flight groups a set of wines to be tasted in a single session by one or more judges.

#### Blind Rating
Judges rate wines on a tablet-optimised screen (**Judge Rating**). Scores are entered for:
- **Appearance (A)** — 0.0 to 3.0
- **Nose (B)** — 0.0 to 4.0
- **Taste (C)** — 0.0 to 13.0

Total score = A + B + C (max 20.0). Comments can be added per wine. Scores auto-save as the judge moves between wines.

The score range labels and gate-value warnings (yellow border + text when a score is below the gate threshold) are dynamically read from the active event's configured gate values.

#### Classification & Results
`IClassificationService` maps total scores to medals using **per-event configurable thresholds** stored on the `Event` document. Admins set thresholds in the event form; `ClassificationService.GetThreshold()` reads the active threshold set from the event config.

| Classification | Norwegian |
|---|---|
| Gold | Gull |
| Silver | Sølv |
| Bronze | Bronse |
| Special Merit | Særlig |
| Acceptable | Akseptabel |
| Not Approved | Ikke godkjent |

`IScoreAggregationService` aggregates individual judge scores into a `WineResult` per wine. An Admin can trigger recalculation from the **Results Report** page.

#### Outlier Detection
`IOutlierDetectionService` identifies judges whose scores deviate statistically from the panel average. Outliers can be reviewed and managed in **Outlier Management**.

#### Trophies
`ITrophyService` determines trophy winners (best Norwegian wine, best Nordic wine) from the aggregated results. Admins can recalculate and view results in **Trophy Reports**.

#### Reports
The **Reports** page uses `IReportService` to produce a flat table of averaged scores, totals and classifications per wine — suitable for printing or exporting.

#### Export & PDF
`IExportService` and `IPdfService` support data export and PDF generation of results and reports.

#### Public Results
A public-facing page (**Public Results**) shows final results without requiring authentication.

---

## Technical Architecture

### Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| UI | Blazor Server |
| Database | MongoDB (via MongoDB.Driver 3.6.0) |
| Authentication | ASP.NET Core Identity + AspNetCore.Identity.MongoDbCore 7.0.0 |
| Styling | Bootstrap (CSS) |
| Unit tests | xUnit 2.x + Moq 4.x + Shouldly 4.x |

### Project Layout

```
WineApp/
├── src/WineApp/          # Main application
│   ├── Models/           # Domain models, enums, view models
│   ├── Data/             # Repository interfaces + MongoDB implementations
│   ├── Services/         # Business logic services
│   ├── Pages/            # Blazor Server pages (.razor)
│   ├── Shared/           # Shared Blazor components
│   ├── Extensions/       # Download/endpoint extensions
│   ├── Controllers/      # Empty — no REST API
│   ├── Program.cs        # App entry point, DI registration, seeding
│   └── appsettings.json  # MongoDB connection string and DB name
└── tests/WineApp.Tests/  # Unit tests (no MongoDB or Blazor needed)
```

### Data Layer
- **Pattern**: Repository pattern. Interfaces live in `Data/`; implementations wrap `IMongoCollection<T>` sourced from `WineMongoDbContext`.
- **Context**: `WineMongoDbContext` is registered as a singleton. All repositories are scoped.
- **Default connection**: `mongodb://localhost:27017`, database `wineapp`.
- **Models** use BSON attributes (`[BsonId]`, `[BsonRepresentation(BsonType.ObjectId)]`) and nullable reference types. IDs are MongoDB ObjectIds stored as strings.

Repositories:

| Interface | Responsibility |
|---|---|
| `IWineRepository` | Wine CRUD |
| `IWineProducerRepository` | Producer CRUD |
| `IWineRatingRepository` | Judge rating CRUD |
| `IWineResultRepository` | Computed result CRUD |
| `IEventRepository` | Event CRUD, active event lookup |
| `IFlightRepository` | Flight CRUD |
| `IPaymentRepository` | Payment records |

### Service Layer
Blazor pages inject **facade services** or **domain services** — never repositories directly.

**Facade services** (aggregate multiple repositories):

| Service | Pages |
|---|---|
| `IWineCatalogService` | Most pages — wine + producer operations |
| `IWineEventService` | Events, EventDetails |
| `IWineJudgingService` | Ratings, Results |

**Domain services**:

| Service | Responsibility |
|---|---|
| `IClassificationService` | Medal/classification logic |
| `IScoreAggregationService` | Recalculates event results |
| `IReportService` | Aggregates scores into report rows |
| `ITrophyService` | Determines trophy winners |
| `IOutlierDetectionService` | Statistical outlier detection |
| `IWineNumberService` | Sequential wine number assignment |
| `IWineValidationService` | Wine registration business rules |
| `IFlightService` | Flight management |
| `IExportService` | Data export |
| `IPdfService` | PDF generation |
| `CurrentUserState` | Scoped; holds resolved identity for the current Blazor circuit |

### Clock Abstraction
Services never call `DateTime.Now` or `DateTime.UtcNow` directly. `TimeProvider` is injected and registered as a singleton (`TimeProvider.System`). Tests use `FrozenTimeProvider` to pin time deterministically.

### Authentication
- Identity is backed by MongoDB via `AspNetCore.Identity.MongoDbCore`.
- User model: `ApplicationUser : MongoIdentityUser<Guid>` (adds `DisplayName`).
- Cookie auth with 8-hour sliding expiration.
- All Blazor pages are protected with `@attribute [Authorize]` (or role-specific variants). Unauthenticated requests are redirected to `/Account/Login`.

### Seeded Accounts (first run only)

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

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MongoDB running on `mongodb://localhost:27017` (or update `appsettings.json`)

### Build & Run

```bash
cd WineApp/src/WineApp
dotnet restore
dotnet build WineApp.csproj
dotnet run
```

The app is available at `https://localhost:5001` (or `http://localhost:5000`).

On first run, `Program.cs` seeds all roles, user accounts, sample wine producers, wines, and ratings into MongoDB if the collections are empty.

### Run Tests

```bash
cd WineApp
dotnet test tests/WineApp.Tests/WineApp.Tests.csproj
```

Tests are pure unit tests — no MongoDB or running Blazor server required. All repositories are mocked with Moq. Assertions use **Shouldly** (MIT licence).
