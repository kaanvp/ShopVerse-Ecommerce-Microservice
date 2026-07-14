FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/Notification/ShopVerse.Notification.API/ShopVerse.Notification.API.csproj"
RUN dotnet publish "src/Services/Notification/ShopVerse.Notification.API/ShopVerse.Notification.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopVerse.Notification.API.dll"]
