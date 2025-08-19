FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SchemAnalyser/SchemAnalyser.csproj", "SchemAnalyser/"]
RUN dotnet restore "SchemAnalyser/SchemAnalyser.csproj"
COPY . .
WORKDIR "/src/SchemAnalyser"
RUN dotnet build "./SchemAnalyser.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchemAnalyser.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchemAnalyser.dll"]
