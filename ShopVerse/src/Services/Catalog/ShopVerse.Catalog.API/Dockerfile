FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/Catalog/ShopVerse.Catalog.API/ShopVerse.Catalog.API.csproj"
RUN dotnet publish "src/Services/Catalog/ShopVerse.Catalog.API/ShopVerse.Catalog.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopVerse.Catalog.API.dll"]
