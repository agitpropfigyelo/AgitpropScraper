FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["docker.csproj", "docker/"]
RUN dotnet restore "docker/docker.csproj"

COPY . docker/.
RUN dotnet publish -c Release -o /app docker/docker.csproj 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "docker.dll"]