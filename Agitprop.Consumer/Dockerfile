FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY *.csproj ./
COPY ../Agitprop.Core/*.csproj ./Agitprop.Core/
COPY ../Agitprop.Infrastructure.InMemory/*.csproj ./Agitprop.Infrastructure.InMemory/
COPY ../Agitprop.Infrastructure.Puppeteer/*.csproj ./Agitprop.Infrastructure.Puppeteer/
COPY ../Agitprop.Infrastructure.SurrealDB/*.csproj ./Agitprop.Infrastructure.SurrealDB/
COPY ../Agitprop.Scrapers/*.csproj ./Agitprop.Scrapers/

RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Agitprop.Consumer.exe"]
