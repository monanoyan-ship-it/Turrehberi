FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/ErkanTatilPlani.Core/ErkanTatilPlani.Core.csproj ErkanTatilPlani.Core/
COPY src/ErkanTatilPlani.Data/ErkanTatilPlani.Data.csproj ErkanTatilPlani.Data/
COPY src/ErkanTatilPlani.API/ErkanTatilPlani.API.csproj ErkanTatilPlani.API/
RUN dotnet restore ErkanTatilPlani.API/ErkanTatilPlani.API.csproj

COPY src/ErkanTatilPlani.Core/ ErkanTatilPlani.Core/
COPY src/ErkanTatilPlani.Data/ ErkanTatilPlani.Data/
COPY src/ErkanTatilPlani.API/ ErkanTatilPlani.API/
RUN dotnet publish ErkanTatilPlani.API/ErkanTatilPlani.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ErkanTatilPlani.API.dll"]
