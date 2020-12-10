#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HealthMonitor.WebApplication/HealthMonitor.WebApplication.csproj", "HealthMonitor.WebApplication/"]
RUN dotnet restore "HealthMonitor.WebApplication/HealthMonitor.WebApplication.csproj"
COPY . .
WORKDIR "/src/HealthMonitor.WebApplication"
RUN dotnet build "HealthMonitor.WebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthMonitor.WebApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthMonitor.WebApplication.dll"]