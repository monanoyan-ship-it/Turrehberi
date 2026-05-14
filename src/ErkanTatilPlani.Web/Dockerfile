FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/ErkanTatilPlani.Core/ErkanTatilPlani.Core.csproj ErkanTatilPlani.Core/
COPY src/ErkanTatilPlani.Data/ErkanTatilPlani.Data.csproj ErkanTatilPlani.Data/
COPY src/ErkanTatilPlani.Web/ErkanTatilPlani.Web.csproj ErkanTatilPlani.Web/
RUN dotnet restore ErkanTatilPlani.Web/ErkanTatilPlani.Web.csproj

COPY src/ErkanTatilPlani.Core/ ErkanTatilPlani.Core/
COPY src/ErkanTatilPlani.Data/ ErkanTatilPlani.Data/
COPY src/ErkanTatilPlani.Web/ ErkanTatilPlani.Web/
RUN dotnet publish ErkanTatilPlani.Web/ErkanTatilPlani.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ErkanTatilPlani.Web.dll"]
