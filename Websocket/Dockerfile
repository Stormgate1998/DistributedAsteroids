FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["Websocket/Websocket.csproj", "Websocket/"]
RUN dotnet restore "Websocket/Websocket.csproj"

COPY . .
WORKDIR /source/Websocket
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Websocket.dll"]
