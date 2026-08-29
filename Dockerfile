# ================================
# Build stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first for better Docker layer caching
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/API/API.csproj src/API/

# Restore dependencies
RUN dotnet restore src/API/API.csproj

# Copy source code
COPY src/ src/

# Build
RUN dotnet build src/API/API.csproj \
    -c Release

# Publish
RUN dotnet publish src/API/API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ================================
# Runtime stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "API.dll"]