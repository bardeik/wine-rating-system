# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached unless .csproj changes)
COPY ["WineApp.csproj", "."]
RUN dotnet restore "WineApp.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "WineApp.csproj" -c Release -o /app/publish --no-restore

# Blazor framework JS files (blazor.server.js etc.) are published to
# wwwroot/_framework/ on Windows but may not be on Linux.  Locate them in
# the NuGet package cache (populated during restore) and copy them across.
RUN mkdir -p /app/publish/wwwroot/_framework && \
    find /root/.nuget /root/.local -name "blazor.server.js" 2>/dev/null \
      | head -1 \
      | xargs -I{} sh -c 'cp {} /app/publish/wwwroot/_framework/blazor.server.js && echo "Copied blazor.server.js"' \
    || echo "blazor.server.js not found in NuGet cache – already in wwwroot or served via manifest"

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Fly.io routes external HTTPS → internal HTTP on 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "WineApp.dll"]
