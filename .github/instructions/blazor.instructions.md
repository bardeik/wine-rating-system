---
description: 'Blazor component and application patterns'
applyTo: '**/*.razor, **/*.razor.cs, **/*.razor.css'
---

## Blazor Code Style and Structure

- Write idiomatic and efficient Blazor and C# code.
- Follow .NET and Blazor conventions.
- Use Razor Components appropriately for component-based UI development.
- Prefer inline functions for smaller components but separate complex logic into code-behind or service classes.
- Async/await should be used where applicable to ensure non-blocking UI operations.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Blazor and .NET Specific Guidelines

- Utilize Blazor's built-in features for component lifecycle (e.g., OnInitializedAsync, OnParametersSetAsync).
- Use data binding effectively with @bind.
- Leverage Dependency Injection for services in Blazor.
- Structure Blazor components and services following Separation of Concerns.
- Always use the latest version C#, currently C# 13 features like record types, pattern matching, and global usings.

## Error Handling and Validation

- Implement proper error handling for Blazor pages and API calls.
- Use logging for error tracking in the backend and consider capturing UI-level errors in Blazor with tools like ErrorBoundary.
- Implement validation using FluentValidation or DataAnnotations in forms.

## Blazor API and Performance Optimization

- Utilize Blazor server-side or WebAssembly optimally based on the project requirements.
- Use asynchronous methods (async/await) for API calls or UI actions that could block the main thread.
- Optimize Razor components by reducing unnecessary renders and using StateHasChanged() efficiently.
- Minimize the component render tree by avoiding re-renders unless necessary, using ShouldRender() where appropriate.
- Use EventCallbacks for handling user interactions efficiently, passing only minimal data when triggering events.

## Caching Strategies

- Implement in-memory caching for frequently used data. Use `IMemoryCache` for lightweight caching solutions in Blazor Server apps.
- Consider Distributed Cache strategies (like Redis or SQL Server Cache) for larger applications that need shared state across multiple users or clients.
- Cache service calls by storing responses in component-level fields to avoid redundant database calls during a single page lifecycle.

## State Management

- Use Blazor's built-in Cascading Parameters and EventCallbacks for basic state sharing across components.
- For server-side Blazor, use Scoped Services and the StateContainer pattern to manage state within user sessions while minimizing re-renders.
- Inject `CurrentUserState` (scoped service) to access the resolved identity for the current Blazor circuit; always call `await EnsureInitializedAsync()` in `OnInitializedAsync`.

## Service Integration

- This is a Blazor Server application with **no REST API**. The `Controllers/` folder is intentionally empty.
- Blazor pages communicate with the data layer exclusively through injected services — never inject repositories directly into pages.
- Use `@inject IMyService MyService` in Razor components to obtain service instances via the DI container.
- Implement error handling for service calls using try-catch and provide proper user feedback via the `StatusAlert` shared component.

## Testing and Debugging

- Run unit tests with `dotnet test` from the command line or any IDE.
- Test Blazor services using xUnit; use Moq for mocking repository dependencies and Shouldly for readable assertions.
- Use `FrozenTimeProvider` (defined in `WineApp.Tests`) to pin the clock in time-sensitive tests.
- Debug Blazor UI issues using browser developer tools and your IDE's debugger for server-side issues.

## Security and Authentication

- Implement Authentication and Authorization using ASP.NET Core Identity backed by MongoDB.
- Use HTTPS for all web communication. In production, TLS is terminated at the reverse proxy (Fly.io); `UseHttpsRedirection` is omitted inside the app to avoid redirect loops.
- Protect Blazor pages with `@attribute [Authorize]` or `@attribute [Authorize(Roles = "Admin,Judge")]`.
- Apply security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy, Permissions-Policy) via middleware in `Program.cs`.
