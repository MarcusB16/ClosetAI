# Build context is the repository root: docker build -f backend/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/src/ClosetAPI.csproj backend/src/
RUN dotnet restore backend/src/ClosetAPI.csproj
COPY backend/src backend/src
RUN dotnet publish backend/src/ClosetAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    Media__Folder=/app/media
RUN mkdir -p /app/media && chown $APP_UID:$APP_UID /app/media
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClosetAPI.dll"]
