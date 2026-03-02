# Base stage - Brug .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage - Brug .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Kopier .csproj filer (Sørg for at stierne matcher din mappestruktur)
# Jeg antager her, at din Dockerfile ligger i "src" eller roden.
COPY ["src/MusicPlayer.Api/MusicPlayer.Api.csproj", "src/MusicPlayer.Api/"]
COPY ["src/MusicPlayer.Domain/MusicPlayer.Domain.csproj", "src/MusicPlayer.Domain/"]
COPY ["src/MusicPlayer.Application/MusicPlayer.Application.csproj", "src/MusicPlayer.Application/"]
COPY ["src/MusicPlayer.Infrastructure/MusicPlayer.Infrastructure.csproj", "src/MusicPlayer.Infrastructure/"]

# 2. Restore
RUN dotnet restore "src/MusicPlayer.Api/MusicPlayer.Api.csproj"

# 3. Kopier resten af koden
COPY . .

# 4. Build - Vi går ind i API mappen
WORKDIR "/src/src/MusicPlayer.Api"
RUN dotnet build "MusicPlayer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicPlayer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
# Kopier de publicerede filer fra publish-stadiet
COPY --from=publish /app/publish .

# Sørg for at Media mapperne findes i containeren, så din FileHelper ikke fejler
USER root
RUN mkdir -p /app/Media/Images && mkdir -p /app/Media/Songs && chown -R app:app /app/Media
USER app

ENTRYPOINT ["dotnet", "MusicPlayer.Api.dll"]