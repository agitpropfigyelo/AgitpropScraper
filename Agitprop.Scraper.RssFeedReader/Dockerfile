# Dockerfile for Agitprop.Scraper.RssFeedReader (console app)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["Agitprop.Scraper.RssFeedReader.csproj", "./"]
RUN dotnet restore "Agitprop.Scraper.RssFeedReader.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "Agitprop.Scraper.RssFeedReader.csproj" -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Agitprop.Scraper.RssFeedReader.dll"]
