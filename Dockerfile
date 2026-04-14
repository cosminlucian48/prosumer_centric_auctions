# syntax=docker/dockerfile:1

# Stage 1: restore dependencies with strong layer caching.
# Only the project file is copied first so restore is reused unless dependencies change.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy only project definition first to leverage Docker layer caching for restore.
COPY ProsumerAuctionPlatform.csproj ./
RUN dotnet restore ProsumerAuctionPlatform.csproj

# Stage 2: development image used by docker-compose.
# Source is bind-mounted at runtime; this stage just provides SDK + default watch command.
FROM restore AS dev
WORKDIR /src
COPY . .

# Better file change detection when source is bind-mounted from host (Windows/macOS).
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
CMD ["dotnet", "watch", "--project", "ProsumerAuctionPlatform.csproj", "run"]

# Stage 3: publish optimized release binaries.
FROM restore AS build
WORKDIR /src
COPY . .
RUN dotnet publish ProsumerAuctionPlatform.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Stage 4: minimal runtime image for production-like execution.
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProsumerAuctionPlatform.dll"]