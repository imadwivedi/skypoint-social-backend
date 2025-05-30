# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SkyPointSocial.sln ./
COPY SkyPointSocial.Api/SkyPointSocial.Api.csproj SkyPointSocial.Api/
COPY SkyPointSocial.Application/SkyPointSocial.Application.csproj SkyPointSocial.Application/
COPY SkyPointSocial.Core/SkyPointSocial.Core.csproj SkyPointSocial.Core/

# Restore dependencies
RUN dotnet restore SkyPointSocial.sln

# Copy everything else and publish
COPY . .
WORKDIR /src/SkyPointSocial.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SkyPointSocial.Api.dll"]
