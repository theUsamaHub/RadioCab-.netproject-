# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy entire repository
COPY . .

# Restore using explicit project path
RUN dotnet restore RadioCab/RadioCab.csproj

# Publish
RUN dotnet publish RadioCab/RadioCab.csproj -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RadioCab.dll"]
