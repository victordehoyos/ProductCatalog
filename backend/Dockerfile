FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/WebAPI/ProductCatalogAPI.WebAPI.csproj", "src/WebAPI/"]
COPY ["src/Application/ProductCatalogAPI.Application.csproj", "src/Application/"]
COPY ["src/Domain/ProductCatalogAPI.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/ProductCatalogAPI.Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/WebAPI/ProductCatalogAPI.WebAPI.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/src/WebAPI"
RUN dotnet build "ProductCatalogAPI.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalogAPI.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalogAPI.WebAPI.dll"]