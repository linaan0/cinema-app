# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CinemaApp.Notifications.Domain/CinemaApp.Notifications.Domain.csproj src/CinemaApp.Notifications.Domain/
COPY src/CinemaApp.Notifications.Repository/CinemaApp.Notifications.Repository.csproj src/CinemaApp.Notifications.Repository/
COPY src/CinemaApp.Notifications.Service/CinemaApp.Notifications.Service.csproj src/CinemaApp.Notifications.Service/
COPY src/CinemaApp.Notifications.Api/CinemaApp.Notifications.Api.csproj src/CinemaApp.Notifications.Api/

RUN dotnet restore src/CinemaApp.Notifications.Api/CinemaApp.Notifications.Api.csproj

COPY src/ src/
RUN dotnet publish src/CinemaApp.Notifications.Api/CinemaApp.Notifications.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Notifications.Api.dll"]
