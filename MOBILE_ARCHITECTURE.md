# VinApp — Mobile Architecture

## Overview

VinApp is the mobile companion to the Wine Rating System web application. It is built
with **.NET MAUI Blazor Hybrid**, sharing Razor UI components with the existing Blazor
Server app through a shared Razor Class Library (`WineApp.Shared`).

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                          MOBILE APP LAYER                           │
│                                                                     │
│   ┌──────────────────────────────────────────────────────────────┐  │
│   │               WineApp.Mobile (MAUI Blazor Hybrid)           │  │
│   │  ┌──────────────────────┐   ┌────────────────────────────┐  │  │
│   │  │  Android (net10.0-   │   │   iOS (net10.0-ios)        │  │  │
│   │  │  android)            │   │   Xcode / Apple SDK        │  │  │
│   │  └──────────────────────┘   └────────────────────────────┘  │  │
│   │                                                              │  │
│   │   BlazorWebView (renders Razor components natively)         │  │
│   │   ├── Routes.razor → MobileLayout.razor                     │  │
│   │   ├── Pages/MobileJudgeRating.razor  (judge scoring)        │  │
│   │   ├── Pages/MobileWineList.razor     (wine catalogue)       │  │
│   │   └── Pages/MobileResults.razor      (public results)       │  │
│   │                                                              │  │
│   │   Services (HttpClient implementations)                     │  │
│   │   ├── MobileAuthService    ← IMobileAuthService             │  │
│   │   ├── MobileWineService    ← IMobileWineService             │  │
│   │   ├── MobileRatingService  ← IMobileRatingService           │  │
│   │   ├── MobileEventService   ← IMobileEventService            │  │
│   │   ├── MobileFlightService  ← IMobileFlightService           │  │
│   │   ├── TokenStore           (MAUI SecureStorage)             │  │
│   │   └── AuthTokenHandler     (DelegatingHandler)              │  │
│   └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│   ┌──────────────────────────────────────────────────────────────┐  │
│   │            WineApp.Shared (Razor Class Library)             │  │
│   │  Interfaces: IMobileWineService, IMobileRatingService, …    │  │
│   │  DTOs: WineDto, WineRatingDto, EventDto, MobileUserDto, …   │  │
│   │  Pages: MobileJudgeRating, MobileWineList, MobileResults    │  │
│   │  Layout: MobileLayout                                       │  │
│   │  TFMs: net10.0 | net10.0-android | net10.0-ios             │  │
│   └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                              │  HTTPS REST API
                              │  Authorization: Bearer <token>
┌─────────────────────────────▼───────────────────────────────────────┐
│                        REST API LAYER                               │
│                                                                     │
│   WineApp (ASP.NET Core 10 Blazor Server)                          │
│   ┌──────────────────────────────────────────────────────────────┐  │
│   │  MobileApiExtensions  (Minimal API endpoints)               │  │
│   │  POST /api/mobile/auth/login                                 │  │
│   │  GET  /api/mobile/auth/me                                    │  │
│   │  GET  /api/mobile/events/active                              │  │
│   │  GET  /api/mobile/wines                                      │  │
│   │  GET  /api/mobile/wines/{wineId}                             │  │
│   │  GET  /api/mobile/flights/my                                 │  │
│   │  GET  /api/mobile/ratings/my                                 │  │
│   │  GET  /api/mobile/ratings/wine/{wineId}                      │  │
│   │  POST /api/mobile/ratings                                    │  │
│   └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│   Auth: IDataProtectionProvider token (8 h expiry)                 │
│   Services: IWineCatalogService, IWineJudgingService, IFlightService│
└─────────────────────────────────────────────────────────────────────┘
                              │  MongoDB Driver 3.x
┌─────────────────────────────▼───────────────────────────────────────┐
│                        DATA LAYER                                   │
│                                                                     │
│   MongoDB (Atlas / Fly.io volume)                                  │
│   Collections: Wines, WineRatings, Events, Flights, Users, …       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Cross-Platform Options Comparison

| Criterion                  | .NET MAUI Blazor Hybrid | Xamarin.Forms        | React Native         | Flutter              |
|----------------------------|-------------------------|----------------------|----------------------|----------------------|
| **Language**               | C# / Razor              | C# / XAML            | JavaScript/TypeScript| Dart                 |
| **UI rendering**           | WebView + Blazor         | Native controls      | Native controls      | Skia canvas          |
| **Code reuse w/ web**      | ✅ Razor components shared| ❌ Separate stack    | ❌ Separate stack    | ❌ Separate stack    |
| **Windows dev support**    | ✅ Full VS 2022+         | ✅ (deprecated)      | ✅ (WSL/Node)        | ✅                   |
| **iOS support from Win**   | ✅ Via Mac Build Host    | ✅ Via Mac Build Host| ✅                   | ✅                   |
| **.NET 10 support**        | ✅ First-class           | ❌ End-of-life       | ❌ N/A               | ❌ N/A               |
| **NuGet ecosystem**        | ✅ Full .NET             | ✅ (legacy)          | ⚠️ npm               | ⚠️ pub.dev           |
| **Team skill re-use**      | ✅ Existing C# devs      | ✅                   | ❌ JS/TS required    | ❌ Dart required     |
| **Hot reload**             | ✅                       | ✅                   | ✅                   | ✅                   |
| **Offline capability**     | ✅                       | ✅                   | ✅                   | ✅                   |
| **Maintenance outlook**    | ✅ Actively invested     | ❌ Deprecated 2024   | ✅ Meta-backed        | ✅ Google-backed     |
| **SecureStorage built-in** | ✅ MAUI Essentials       | ✅ Xamarin.Essentials| ⚠️ 3rd party         | ⚠️ 3rd party         |

---

## Why .NET MAUI Blazor Hybrid Was Chosen

1. **Shared Razor components** — The existing `WineApp` already uses Blazor for all
   UI. With MAUI Blazor Hybrid, the exact same Razor component code (`MobileJudgeRating`,
   `MobileWineList`, `MobileLayout`) runs inside a native WebView on mobile, with zero
   JavaScript framework required.

2. **Single C# codebase** — Judges and producers are familiar with the web app's UX.
   The mobile app mirrors that experience. The development team stays in C# throughout:
   models, services, validation, and UI.

3. **WineApp.Shared RCL** — All service interfaces (`IMobileRatingService`, etc.) and
   DTOs live in a single Razor Class Library that is referenced by both the MAUI app
   (for Android/iOS) *and* the server project. This enforces API contract consistency
   at compile time.

4. **MAUI Essentials built-in** — `SecureStorage.Default` (for the auth token) and
   `Connectivity` (for offline detection) are available with no extra packages.

5. **Windows development** — The entire solution can be developed on Windows using
   Visual Studio 2022 (17.8+) or `dotnet` CLI. iOS builds require a networked Mac
   Build Host (standard practice), while Android builds compile entirely on Windows.

6. **Long-term support** — Microsoft is actively investing in MAUI as the strategic
   cross-platform .NET UI framework. Xamarin is end-of-life. MAUI ships with .NET 10.

---

## Development Setup (Windows)

### Prerequisites

```
1. Visual Studio 2022 (17.10+) with workloads:
   - ".NET Multi-platform App UI development"
   - "ASP.NET and web development"

   OR: Visual Studio Code + .NET 10 SDK + MAUI workload:
   dotnet workload install maui

2. For Android emulation:
   - Android SDK (installed by VS Installer automatically)
   - Android emulator image (API 33+)

3. For iOS (requires a Mac):
   - Mac with Xcode 15+
   - Pair to Mac from Visual Studio: Tools → iOS → Pair to Mac
```

### First-time build

```bash
# Install MAUI workload (if not done during VS install)
dotnet workload install maui

# Restore all projects
dotnet restore WineApp/WineApp.sln

# Build the server (Linux/CI — always works)
dotnet build WineApp/src/WineApp/WineApp.csproj

# Build Android (Windows or Linux with Android SDK)
dotnet build WineApp/src/WineApp.Mobile/WineApp.Mobile.csproj \
    -f net10.0-android

# Build iOS (Mac only, requires Xcode)
dotnet build WineApp/src/WineApp.Mobile/WineApp.Mobile.csproj \
    -f net10.0-ios
```

### Configuration

Edit `WineApp.Mobile/AppConfig.cs` (or inject via environment) to point to your
deployed server:

```csharp
BaseUrl = "https://your-wineapp-server.fly.dev"
```

For local development, use the server's HTTPS URL from `launchSettings.json` and
trust the dev cert on the emulator.

---

## Project Structure

```
WineApp.sln
├── src/
│   ├── WineApp/                        ← Blazor Server (existing)
│   │   ├── Extensions/
│   │   │   ├── DownloadEndpointExtensions.cs
│   │   │   └── MobileApiExtensions.cs  ← NEW: REST endpoints for mobile
│   │   ├── Models/                     ← Domain models (MongoDB)
│   │   ├── Services/                   ← Business logic
│   │   ├── Data/                       ← Repository interfaces + MongoDB impl
│   │   └── Program.cs
│   │
│   ├── WineApp.Shared/                 ← NEW: Razor Class Library
│   │   ├── Dtos/                       ← Mobile DTOs (no MongoDB attributes)
│   │   │   ├── WineDto.cs
│   │   │   ├── WineRatingDto.cs
│   │   │   ├── EventDto.cs
│   │   │   ├── MobileUserDto.cs
│   │   │   ├── LoginRequestDto.cs
│   │   │   └── LoginResponseDto.cs
│   │   ├── MobileServices/             ← Service interfaces
│   │   │   ├── IMobileWineService.cs
│   │   │   ├── IMobileRatingService.cs
│   │   │   ├── IMobileEventService.cs
│   │   │   ├── IMobileFlightService.cs
│   │   │   └── IMobileAuthService.cs
│   │   ├── Pages/                      ← Shared Razor components
│   │   │   ├── MobileJudgeRating.razor
│   │   │   ├── MobileWineList.razor
│   │   │   └── MobileResults.razor
│   │   ├── Shared/
│   │   │   ├── MobileLayout.razor
│   │   │   └── MobileLayout.razor.css
│   │   └── _Imports.razor
│   │
│   └── WineApp.Mobile/                 ← NEW: .NET MAUI Blazor Hybrid app
│       ├── Services/                   ← HttpClient implementations
│       │   ├── MobileAuthService.cs
│       │   ├── MobileWineService.cs
│       │   ├── MobileRatingService.cs
│       │   ├── MobileEventService.cs
│       │   ├── MobileFlightService.cs
│       │   ├── TokenStore.cs
│       │   └── AuthTokenHandler.cs
│       ├── Platforms/
│       │   ├── Android/
│       │   │   ├── MainActivity.cs
│       │   │   ├── MainApplication.cs
│       │   │   └── AndroidManifest.xml
│       │   └── iOS/
│       │       ├── AppDelegate.cs
│       │       └── Info.plist
│       ├── Resources/
│       │   ├── AppIcon/
│       │   ├── Splash/
│       │   ├── Fonts/
│       │   └── Styles/
│       │       ├── Colors.xaml
│       │       └── Styles.xaml
│       ├── wwwroot/
│       │   └── index.html
│       ├── App.xaml / App.xaml.cs
│       ├── AppConfig.cs
│       ├── MainPage.xaml / MainPage.xaml.cs
│       ├── MauiProgram.cs
│       └── Routes.razor
│
└── tests/
    └── WineApp.Tests/                  ← xUnit test project (existing)
```

---

## Mobile API Auth Flow

```
Mobile App                        Server (WineApp)
    │                                     │
    │  POST /api/mobile/auth/login        │
    │  { email, password }                │
    │ ──────────────────────────────────► │
    │                                     │  SignInManager.CheckPasswordSignInAsync
    │                                     │  IDataProtector.Protect("{userId}|{expiry:O}")
    │  200 { token, displayName, roles }  │
    │ ◄────────────────────────────────── │
    │                                     │
    │  SecureStorage.SetAsync(token)      │
    │                                     │
    │  GET /api/mobile/wines              │
    │  Authorization: Bearer <token>      │
    │ ──────────────────────────────────► │
    │                                     │  ValidateMobileToken(token)
    │                                     │   → IDataProtector.Unprotect
    │                                     │   → parse userId + expiry
    │                                     │   → UserManager.FindByIdAsync
    │  200 [ { WineDto }, ... ]           │
    │ ◄────────────────────────────────── │
```

## Key Design Decisions

- **Token format**: `IDataProtectionProvider` with purpose `"MobileAuth"`. No JWT dependency needed — `IDataProtection` is already registered in ASP.NET Core. Token lifetime is 8 hours (matching the web cookie).
- **No session created on login**: `CheckPasswordSignInAsync` validates without writing a cookie, keeping the mobile API stateless.
- **DTOs vs. domain models**: DTOs in `WineApp.Shared` intentionally have no MongoDB BSON attributes — they are safe to serialize with `System.Text.Json` and reference from the MAUI project without a MongoDB driver dependency.
- **SecureStorage for tokens**: `Microsoft.Maui.Storage.SecureStorage` is backed by Android Keystore and iOS Keychain — appropriate for auth tokens.
