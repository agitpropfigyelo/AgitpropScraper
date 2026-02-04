# Dockerfile for Agitprop.Web.API (ASP.NET Core Web API)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["Agitprop.Web.API.csproj", "./"]
RUN dotnet restore "Agitprop.Web.API.csproj"

# copy and publish
COPY . .
RUN dotnet publish "Agitprop.Web.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Agitprop.Web.API.dll"]
