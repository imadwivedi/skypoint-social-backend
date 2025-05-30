# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and project files first to leverage Docker layer caching
COPY SkyPointSocial.sln .
COPY SkyPointSocial.Api/SkyPointSocial.Api.csproj SkyPointSocial.Api/
COPY SkyPointSocial.Application/SkyPointSocial.Application.csproj SkyPointSocial.Application/
COPY SkyPointSocial.Core/SkyPointSocial.Core.csproj SkyPointSocial.Core/
RUN dotnet restore SkyPointSocial.sln

COPY . .

# Build and publish the API project
WORKDIR /src/SkyPointSocial.Api

# Publish the API project
RUN dotnet publish SkyPointSocial.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkyPointSocial.Api.dll"]