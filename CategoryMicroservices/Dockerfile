FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CategoryMicroservices/CategoryMicroservices.csproj", "CategoryMicroservices/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

RUN dotnet restore "CategoryMicroservices/CategoryMicroservices.csproj"

COPY ["CategoryMicroservices/", "CategoryMicroservices/"]
COPY ["SharedLibrary/", "SharedLibrary/"]
COPY ["Protos/", "Protos/"]

WORKDIR "/src/CategoryMicroservices"
RUN dotnet publish "CategoryMicroservices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CategoryMicroservices.dll"]