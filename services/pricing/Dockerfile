# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CinemaApp.Pricing.Domain/CinemaApp.Pricing.Domain.csproj src/CinemaApp.Pricing.Domain/
COPY src/CinemaApp.Pricing.Repository/CinemaApp.Pricing.Repository.csproj src/CinemaApp.Pricing.Repository/
COPY src/CinemaApp.Pricing.Service/CinemaApp.Pricing.Service.csproj src/CinemaApp.Pricing.Service/
COPY src/CinemaApp.Pricing.Api/CinemaApp.Pricing.Api.csproj src/CinemaApp.Pricing.Api/

RUN dotnet restore src/CinemaApp.Pricing.Api/CinemaApp.Pricing.Api.csproj

COPY src/ src/
RUN dotnet publish src/CinemaApp.Pricing.Api/CinemaApp.Pricing.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CinemaApp.Pricing.Api.dll"]
