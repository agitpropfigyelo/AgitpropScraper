FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy the project files
COPY ["./Agitprop.Consumer/Agitprop.Consumer.csproj", "Agitprop.Consumer/"]
COPY ["./Agitprop.Core/Agitprop.Core.csproj", "Agitprop.Core/"]
COPY ["./Agitprop.Infrastructure.InMemory/Agitprop.Infrastructure.InMemory.csproj", "Agitprop.Infrastructure.InMemory/"]
COPY ["./Agitprop.Infrastructure.Puppeteer/Agitprop.Infrastructure.Puppeteer.csproj", "Agitprop.Infrastructure.Puppeteer/"]
COPY ["./Agitprop.Infrastructure.SurrealDB/Agitprop.Infrastructure.SurrealDB.csproj", "Agitprop.Infrastructure.SurrealDB/"]
COPY ["./Agitprop.Scrapers/Agitprop.Scrapers.csproj", "Agitprop.Scrapers/"]

# Restore as distinct layers
RUN dotnet restore "Agitprop.Consumer/Agitprop.Consumer.csproj"

# Copy the remaining files and build the application
COPY . .
WORKDIR /App/Agitprop.Consumer
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y locales \
    && localedef -i en_US -f ISO-8859-2 en_US.ISO-8859-2
WORKDIR /App
COPY --from=build-env /App/Agitprop.Consumer/out .
ENTRYPOINT ["dotnet", "Agitprop.Consumer.dll"]