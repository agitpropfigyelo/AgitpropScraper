# Dockerfile for Agitprop.Web.API (ASP.NET Core Web API)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy the full repo so Directory.Packages.props is available and restore
COPY . .
WORKDIR /src/Agitprop.Web.API
RUN dotnet restore "Agitprop.Web.API.csproj"

# publish (restore already done)
RUN dotnet publish "Agitprop.Web.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Agitprop.Web.API.dll"]
