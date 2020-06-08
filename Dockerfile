FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["TgtgWatcherService/TgtgWatcherService.csproj", "TgtgWatcherService/"]
RUN dotnet restore "TgtgWatcherService/TgtgWatcherService.csproj"
COPY . .
WORKDIR "/src/TgtgWatcherService"
RUN dotnet build "TgtgWatcherService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TgtgWatcherService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TgtgWatcherService.dll"]