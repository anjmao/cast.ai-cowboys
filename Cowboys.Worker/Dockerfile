FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Cowboys.Worker/Cowboys.Worker.csproj", "Cowboys.Worker/"]
COPY ["RMQ/RMQ.csproj", "RMQ/"]
COPY ["Cowboys.Telemetry/Cowboys.Telemetry.csproj", "Cowboys.Telemetry/"]
COPY ["Cowboys.Http.Sdk/Cowboys.Http.Sdk.csproj", "Cowboys.Http.Sdk/"]
COPY ["Cowboys.Contracts/Cowboys.Contracts.csproj", "Cowboys.Contracts/"]
RUN dotnet restore "Cowboys.Worker/Cowboys.Worker.csproj"
COPY . .
WORKDIR "/src/Cowboys.Worker"
RUN dotnet build "Cowboys.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cowboys.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cowboys.Worker.dll"]
