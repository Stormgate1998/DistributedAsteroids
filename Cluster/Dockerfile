FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["Cluster/Cluster.csproj", "Cluster/"]
RUN dotnet restore "Cluster/Cluster.csproj"

COPY . .
WORKDIR /source/Cluster
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Cluster.dll"]
