# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CinemaApp.Auth.Domain/CinemaApp.Auth.Domain.csproj src/CinemaApp.Auth.Domain/
COPY src/CinemaApp.Auth.Repository/CinemaApp.Auth.Repository.csproj src/CinemaApp.Auth.Repository/
COPY src/CinemaApp.Auth.Service/CinemaApp.Auth.Service.csproj src/CinemaApp.Auth.Service/
COPY src/CinemaApp.Auth.Api/CinemaApp.Auth.Api.csproj src/CinemaApp.Auth.Api/

RUN dotnet restore src/CinemaApp.Auth.Api/CinemaApp.Auth.Api.csproj

COPY src/ src/
RUN dotnet publish src/CinemaApp.Auth.Api/CinemaApp.Auth.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Auth.Api.dll"]
