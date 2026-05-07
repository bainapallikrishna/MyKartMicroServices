FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserMicroservices/UserMicroservices.csproj", "UserMicroservices/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

RUN dotnet restore "UserMicroservices/UserMicroservices.csproj"

COPY ["UserMicroservices/", "UserMicroservices/"]
COPY ["SharedLibrary/", "SharedLibrary/"]
COPY ["Protos/", "Protos/"]

WORKDIR "/src/UserMicroservices"
RUN dotnet publish "UserMicroservices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserMicroservices.dll"]