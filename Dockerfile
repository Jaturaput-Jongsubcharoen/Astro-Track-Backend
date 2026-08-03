FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AstroTrack.sln ./
COPY AstroTrack.Api/AstroTrack.Api.csproj AstroTrack.Api/
COPY AstroTrack.Api.Tests/AstroTrack.Api.Tests.csproj AstroTrack.Api.Tests/

RUN dotnet restore AstroTrack.sln

COPY . .
RUN dotnet publish AstroTrack.Api/AstroTrack.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

USER app

EXPOSE 5000

ENTRYPOINT ["dotnet", "AstroTrack.Api.dll"]