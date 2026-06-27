# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CinemaApp.Movies.Domain/CinemaApp.Movies.Domain.csproj src/CinemaApp.Movies.Domain/
COPY src/CinemaApp.Movies.Repository/CinemaApp.Movies.Repository.csproj src/CinemaApp.Movies.Repository/
COPY src/CinemaApp.Movies.Service/CinemaApp.Movies.Service.csproj src/CinemaApp.Movies.Service/
COPY src/CinemaApp.Movies.Api/CinemaApp.Movies.Api.csproj src/CinemaApp.Movies.Api/

RUN dotnet restore src/CinemaApp.Movies.Api/CinemaApp.Movies.Api.csproj

COPY src/ src/
RUN dotnet publish src/CinemaApp.Movies.Api/CinemaApp.Movies.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Movies.Api.dll"]
