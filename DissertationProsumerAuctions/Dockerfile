# Use the base .NET 6 SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set working directory
WORKDIR /app

# Copy the .NET 6 console application project files
COPY . .

# Restore dependencies and build the application
RUN dotnet restore
RUN dotnet build -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out .

# Set entrypoint to run the executable
CMD ["dotnet", "DissertationProsumerAuctions.dll"]