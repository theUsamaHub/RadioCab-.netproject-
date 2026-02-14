# Use official .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY RadioCab/ RadioCab/

# Restore dependencies
RUN dotnet restore

# Build the project in Release mode
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "RadioCab.dll"]
