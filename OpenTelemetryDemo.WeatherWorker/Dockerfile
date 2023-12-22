FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OpeneTelemetryDemo.WeatherWorker/OpeneTelemetryDemo.WeatherWorker.csproj", "OpeneTelemetryDemo.WeatherWorker/"]
RUN dotnet restore "OpeneTelemetryDemo.WeatherWorker/OpeneTelemetryDemo.WeatherWorker.csproj"
COPY . .
WORKDIR "/src/OpeneTelemetryDemo.WeatherWorker"
RUN dotnet build "OpeneTelemetryDemo.WeatherWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpeneTelemetryDemo.WeatherWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpeneTelemetryDemo.WeatherWorker.dll"]
