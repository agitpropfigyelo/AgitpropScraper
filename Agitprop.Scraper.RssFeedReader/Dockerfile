# Dockerfile for Agitprop.Scraper.RssFeedReader (console app)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy the full repo so Directory.Packages.props is available and restore
COPY . .
WORKDIR /src/Agitprop.Scraper.RssFeedReader
RUN dotnet restore "Agitprop.Scraper.RssFeedReader.csproj"

# publish (restore already done)
RUN dotnet publish "Agitprop.Scraper.RssFeedReader.csproj" -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Agitprop.Scraper.RssFeedReader.dll"]
