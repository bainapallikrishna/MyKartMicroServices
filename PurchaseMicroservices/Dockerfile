# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only csproj first (cache optimization)
COPY ["PurchaseMicroservices/PurchaseMicroservices.csproj", "PurchaseMicroservices/"]
RUN dotnet restore "PurchaseMicroservices/PurchaseMicroservices.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/PurchaseMicroservices"
RUN dotnet publish "PurchaseMicroservices.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PurchaseMicroservices.dll"]