﻿FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OpenTelemetryDemo.Database/OpenTelemetryDemo.Database.csproj", "OpenTelemetryDemo.Database/"]
RUN dotnet restore "OpenTelemetryDemo.Database/OpenTelemetryDemo.Database.csproj"
COPY . .
WORKDIR "/src/OpenTelemetryDemo.Database"
RUN dotnet build "OpenTelemetryDemo.Database.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenTelemetryDemo.Database.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenTelemetryDemo.Database.dll"]
