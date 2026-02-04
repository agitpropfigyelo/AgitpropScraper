# Dockerfile for Agitprop.Scraper.Consumer (background service)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["Agitprop.Scraper.Consumer.csproj", "./"]
RUN dotnet restore "Agitprop.Scraper.Consumer.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "Agitprop.Scraper.Consumer.csproj" -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Agitprop.Scraper.Consumer.dll"]
