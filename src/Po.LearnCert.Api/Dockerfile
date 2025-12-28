# Build stage - Use .NET 9 SDK (avoids MSBuild GetTargetFrameworkVersion bug in SDK 10.0.101)
# The net10.0 target will still be built correctly via forward compatibility
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-noble-amd64 AS build
WORKDIR /src

# Copy solution and project files
COPY Directory.Packages.props ./
COPY src/Po.LearnCert.Api/Po.LearnCert.Api.csproj src/Po.LearnCert.Api/
COPY src/Po.LearnCert.Client/Po.LearnCert.Client.csproj src/Po.LearnCert.Client/
COPY src/Po.LearnCert.Shared/Po.LearnCert.Shared.csproj src/Po.LearnCert.Shared/
COPY src/Po.LearnCert.ServiceDefaults/Po.LearnCert.ServiceDefaults.csproj src/Po.LearnCert.ServiceDefaults/

# Restore
RUN dotnet restore src/Po.LearnCert.Api/Po.LearnCert.Api.csproj

# Copy source
COPY src/ src/

# Build and publish
RUN dotnet publish src/Po.LearnCert.Api/Po.LearnCert.Api.csproj -c Release -o /app/publish

# Runtime stage - .NET 10 GA
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Po.LearnCert.Api.dll"]
