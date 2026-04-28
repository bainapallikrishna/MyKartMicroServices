FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ProductMicroservices/ProductMicroservices.csproj", "ProductMicroservices/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]
RUN dotnet restore "ProductMicroservices/ProductMicroservices.csproj"

COPY ["ProductMicroservices/", "ProductMicroservices/"]
COPY ["SharedLibrary/", "SharedLibrary/"]
COPY ["Protos/", "Protos/"]

WORKDIR "/src/ProductMicroservices"
RUN dotnet publish "ProductMicroservices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductMicroservices.dll"]