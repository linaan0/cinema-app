# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CinemaApp.Bookings.Domain/CinemaApp.Bookings.Domain.csproj src/CinemaApp.Bookings.Domain/
COPY src/CinemaApp.Bookings.Repository/CinemaApp.Bookings.Repository.csproj src/CinemaApp.Bookings.Repository/
COPY src/CinemaApp.Bookings.Service/CinemaApp.Bookings.Service.csproj src/CinemaApp.Bookings.Service/
COPY src/CinemaApp.Bookings.Api/CinemaApp.Bookings.Api.csproj src/CinemaApp.Bookings.Api/

RUN dotnet restore src/CinemaApp.Bookings.Api/CinemaApp.Bookings.Api.csproj

COPY src/ src/
RUN dotnet publish src/CinemaApp.Bookings.Api/CinemaApp.Bookings.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Bookings.Api.dll"]
