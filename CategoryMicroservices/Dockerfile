# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first (cache optimization)
COPY ["CategoryMicroservices/CategoryMicroservices.csproj", "CategoryMicroservices/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

# Restore
RUN dotnet restore "CategoryMicroservices/CategoryMicroservices.csproj"

# Copy source files
COPY ["CategoryMicroservices/", "CategoryMicroservices/"]
COPY ["SharedLibrary/", "SharedLibrary/"]

# Copy Protos
COPY ["Protos/", "Protos/"]

WORKDIR "/src/CategoryMicroservices"
RUN dotnet publish "CategoryMicroservices.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CategoryMicroservices.dll"]