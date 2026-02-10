# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY MercerAssistant.slnx Directory.Build.props ./
COPY src/MercerAssistant.Core/MercerAssistant.Core.csproj src/MercerAssistant.Core/
COPY src/MercerAssistant.Infrastructure/MercerAssistant.Infrastructure.csproj src/MercerAssistant.Infrastructure/
COPY src/MercerAssistant.Web/MercerAssistant.Web.csproj src/MercerAssistant.Web/

RUN dotnet restore MercerAssistant.slnx

# Copy everything else and publish
COPY . .
RUN dotnet publish src/MercerAssistant.Web/MercerAssistant.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MercerAssistant.Web.dll"]
