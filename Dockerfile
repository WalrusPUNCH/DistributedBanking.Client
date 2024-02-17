FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DistributedBanking.Client.API/DistributedBanking.Client.API.csproj", "DistributedBanking.Client.API/"]
COPY ["DistributedBanking.Client.Domain/DistributedBanking.Client.Domain.csproj", "DistributedBanking.Client.Domain/"]
COPY ["DistributedBanking.Client.Data/DistributedBanking.Client.Data.csproj", "DistributedBanking.Client.Data/"]
COPY ["DistributedBanking.Shared/Contracts/Contracts.csproj", "DistributedBanking.Shared/Contracts/"]
COPY ["DistributedBanking.Shared/Shared.Data/Shared.Data.csproj", "DistributedBanking.Shared/Shared.Data/"]
COPY ["DistributedBanking.Shared/Shared.Kafka/Shared.Kafka.csproj", "DistributedBanking.Shared/Shared.Kafka/"]
COPY ["DistributedBanking.Shared/Shared.Messaging/Shared.Messaging.csproj", "DistributedBanking.Shared/Shared.Messaging/"]
COPY ["DistributedBanking.Shared/Shared.Redis/Shared.Redis.csproj", "DistributedBanking.Shared/Shared.Redis/"]
RUN dotnet restore "./DistributedBanking.Client.API/DistributedBanking.Client.API.csproj"
COPY . .
WORKDIR "/src/DistributedBanking.Client.API"
RUN dotnet build "./DistributedBanking.Client.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DistributedBanking.Client.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedBanking.Client.API.dll"]