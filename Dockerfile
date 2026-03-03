# Base stage - Use .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage - Use .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copy .csproj files
# Dockerfile is located in root.
COPY ["src/MusicPlayer.Api/MusicPlayer.Api.csproj", "src/MusicPlayer.Api/"]
COPY ["src/MusicPlayer.Domain/MusicPlayer.Domain.csproj", "src/MusicPlayer.Domain/"]
COPY ["src/MusicPlayer.Application/MusicPlayer.Application.csproj", "src/MusicPlayer.Application/"]
COPY ["src/MusicPlayer.Infrastructure/MusicPlayer.Infrastructure.csproj", "src/MusicPlayer.Infrastructure/"]

# 2. Restore
RUN dotnet restore "src/MusicPlayer.Api/MusicPlayer.Api.csproj"

# 3. Copy the rest of the code
COPY . .

# 4. Build the API
WORKDIR "/src/src/MusicPlayer.Api"
RUN dotnet build "MusicPlayer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicPlayer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
# Copy the published files
COPY --from=publish /app/publish .

# Make sure Media folders exists
USER root
RUN mkdir -p /app/Media/Images && mkdir -p /app/Media/Songs && chown -R app:app /app/Media
USER app

ENTRYPOINT ["dotnet", "MusicPlayer.Api.dll"]