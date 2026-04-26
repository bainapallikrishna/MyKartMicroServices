# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only csproj first (cache optimization)
COPY ["UserMicroservices/UserMicroservices.csproj", "UserMicroservices/"]
RUN dotnet restore "UserMicroservices/UserMicroservices.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/UserMicroservices"
RUN dotnet publish "UserMicroservices.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserMicroservices.dll"]

