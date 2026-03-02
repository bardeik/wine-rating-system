# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cached unless .csproj changes)
COPY ["WineApp.csproj", "."]
RUN dotnet restore "WineApp.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "WineApp.csproj" -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# The Linux SDK does not include _framework/blazor.server.js in the publish output.
# The file lives in the ASP.NET shared framework under /usr/share/dotnet.
# Copy it (and compressed variants) to wwwroot/_framework so UseStaticFiles serves it.
RUN mkdir -p wwwroot/_framework && \
    find /usr/share/dotnet -name "blazor.server.js"    | head -1 | xargs -I{} cp {} wwwroot/_framework/ && \
    find /usr/share/dotnet -name "blazor.server.js.gz" | head -1 | xargs -I{} cp {} wwwroot/_framework/ && \
    find /usr/share/dotnet -name "blazor.server.js.br" | head -1 | xargs -I{} cp {} wwwroot/_framework/

# Fly.io routes external HTTPS → internal HTTP on 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "WineApp.dll"]
