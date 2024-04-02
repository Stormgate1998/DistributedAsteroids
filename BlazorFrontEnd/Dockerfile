FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["BlazorFrontEnd/BlazorFrontEnd.csproj", "BlazorFrontEnd/"]
RUN dotnet restore "BlazorFrontEnd/BlazorFrontEnd.csproj"

COPY . .
WORKDIR /source/BlazorFrontEnd
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BlazorFrontEnd.dll"]

# FROM nginx:alpine AS runtime
# WORKDIR /usr/share/nginx/html
# RUN rm -rf ./*
# COPY --from=build /app/wwwroot .
# COPY nginx.conf /etc/nginx/nginx.conf