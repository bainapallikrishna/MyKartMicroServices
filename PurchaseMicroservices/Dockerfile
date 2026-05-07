FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PurchaseMicroservices/PurchaseMicroservices.csproj", "PurchaseMicroservices/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

RUN dotnet restore "PurchaseMicroservices/PurchaseMicroservices.csproj"

COPY ["PurchaseMicroservices/", "PurchaseMicroservices/"]
COPY ["SharedLibrary/", "SharedLibrary/"]
COPY ["Protos/", "Protos/"]

WORKDIR "/src/PurchaseMicroservices"
RUN dotnet publish "PurchaseMicroservices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PurchaseMicroservices.dll"]