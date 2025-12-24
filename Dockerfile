# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PoJoker.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .

# Copy all project files
COPY src/Po.Joker/Po.Joker.csproj src/Po.Joker/
COPY src/Po.Joker.Client/Po.Joker.Client.csproj src/Po.Joker.Client/
COPY src/Po.Joker.Shared/Po.Joker.Shared.csproj src/Po.Joker.Shared/
COPY src/PoJoker.ServiceDefaults/PoJoker.ServiceDefaults.csproj src/PoJoker.ServiceDefaults/

# Restore dependencies
RUN dotnet restore src/Po.Joker/Po.Joker.csproj

# Copy everything else
COPY src/ src/

# Build and publish
WORKDIR /src/src/Po.Joker
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Po.Joker.dll"]
