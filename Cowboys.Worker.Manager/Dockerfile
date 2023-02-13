FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Cowboys.Worker.Manager/Cowboys.Worker.Manager.csproj", "Cowboys.Worker.Manager/"]
COPY ["Cowboys.Telemetry/Cowboys.Telemetry.csproj", "Cowboys.Telemetry/"]
COPY ["Cowboys.DataAccess/Cowboys.DataAccess.csproj", "Cowboys.DataAccess/"]
COPY ["Cowboys.Contracts/Cowboys.Contracts.csproj", "Cowboys.Contracts/"]
COPY ["RMQ/RMQ.csproj", "RMQ/"]
RUN dotnet restore "Cowboys.Worker.Manager/Cowboys.Worker.Manager.csproj"
COPY . .
WORKDIR "/src/Cowboys.Worker.Manager"
RUN dotnet build "Cowboys.Worker.Manager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cowboys.Worker.Manager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cowboys.Worker.Manager.dll"]
