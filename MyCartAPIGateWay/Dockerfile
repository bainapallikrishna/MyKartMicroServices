FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MyCartAPIGateWay/MyCartAPIGateWay.csproj", "MyCartAPIGateWay/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]

RUN dotnet restore "MyCartAPIGateWay/MyCartAPIGateWay.csproj"

COPY ["MyCartAPIGateWay/", "MyCartAPIGateWay/"]
COPY ["SharedLibrary/", "SharedLibrary/"]
COPY ["Protos/", "Protos/"]
WORKDIR "/src/MyCartAPIGateWay"
RUN dotnet publish "MyCartAPIGateWay.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCartAPIGateWay.dll"]