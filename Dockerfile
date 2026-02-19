# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY RadioCab.sln ./
COPY RadioCab/*.csproj ./RadioCab/

# Restore
RUN dotnet restore RadioCab.sln

# Copy all source
COPY . .

# Publish
RUN dotnet publish RadioCab/RadioCab.csproj -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RadioCab.dll"]
