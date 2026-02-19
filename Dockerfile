# ---------- Base Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# ---------- Build Stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY RadioCab/*.csproj ./RadioCab/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish project
WORKDIR /src/RadioCab
RUN dotnet publish -c Release -o /app/publish

# ---------- Final Stage ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RadioCab.dll"]
