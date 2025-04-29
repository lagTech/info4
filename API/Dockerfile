# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Pour le managed identity pour dev seulement.
ENV AZURE_CLIENT_ID="7656a5d3-6163-44fb-b6f0-1d8b2772701d"
ENV AZURE_TENANT_ID="2b2a6365-d23a-4e3d-a0be-a44e8f5319b9"
ENV AZURE_CLIENT_SECRET=".Ul8Q~PBnMSsenrIpkDqi~2WNZP~VtbJoKDDDcVD"

# Pour forcer le redirect URL / Valider aussi le launchSetting.json pour le numéro du port ...
ENV ASPNETCORE_URLS=http://+:8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/API.csproj", "API/"]
COPY ["MVC", "MVC/"]

RUN dotnet restore "./API/API.csproj"
COPY ./API ./API
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]