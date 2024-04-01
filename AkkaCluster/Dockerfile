FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["AkkaCluster/AkkaCluster.csproj", "AkkaCluster/"]
RUN dotnet restore "AkkaCluster/AkkaCluster.csproj"

COPY . .
WORKDIR /source/AkkaCluster
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AkkaCluster.dll"]
