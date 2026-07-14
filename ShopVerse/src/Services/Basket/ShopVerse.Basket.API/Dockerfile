FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/Basket/ShopVerse.Basket.API/ShopVerse.Basket.API.csproj"
RUN dotnet publish "src/Services/Basket/ShopVerse.Basket.API/ShopVerse.Basket.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopVerse.Basket.API.dll"]
