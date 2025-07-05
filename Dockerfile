FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . ./

RUN dotnet restore UrlShortener.sln

RUN dotnet build UrlShortener.sln -c Release --no-restore

RUN dotnet publish UrlShortener.Api/UrlShortener.Api.csproj -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5087

EXPOSE 5087

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]