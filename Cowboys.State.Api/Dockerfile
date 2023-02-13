FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Cowboys.State.Api/Cowboys.State.Api.csproj", "Cowboys.State.Api/"]
COPY ["Cowboys.Contracts/Cowboys.Contracts.csproj", "Cowboys.Contracts/"]
COPY ["Cowboys.DataAccess/Cowboys.DataAccess.csproj", "Cowboys.DataAccess/"]
COPY ["Cowboys.Telemetry/Cowboys.Telemetry.csproj", "Cowboys.Telemetry/"]
COPY ["RMQ/RMQ.csproj", "RMQ/"]
RUN dotnet restore "Cowboys.State.Api/Cowboys.State.Api.csproj"
COPY . .
WORKDIR "/src/Cowboys.State.Api"
RUN dotnet build "Cowboys.State.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cowboys.State.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cowboys.State.Api.dll"]
