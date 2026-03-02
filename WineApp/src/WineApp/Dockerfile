# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached unless .csproj changes)
COPY ["WineApp.csproj", "."]
RUN dotnet restore "WineApp.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "WineApp.csproj" -c Release -o /app/publish --no-restore

# The Linux SDK omits _framework/blazor.server.js from the publish output.
# Copy the pre-built files from .docker-assets (built once on Windows and committed)
# so UseStaticFiles can serve them at /_framework/blazor.server.js.
RUN mkdir -p /app/publish/wwwroot/_framework && \
    cp .docker-assets/_framework/blazor.server.js    /app/publish/wwwroot/_framework/ && \
    cp .docker-assets/_framework/blazor.server.js.gz /app/publish/wwwroot/_framework/ && \
    cp .docker-assets/_framework/blazor.server.js.br /app/publish/wwwroot/_framework/

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Fly.io routes external HTTPS → internal HTTP on 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "WineApp.dll"]
