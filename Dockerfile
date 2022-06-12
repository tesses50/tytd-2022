FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app
RUN git clone https://github.com/tesses50/tytd-2022 .
WORKDIR /app/Tesses.YouTubeDownloader.Net6
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
EXPOSE 3252
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Tesses.YouTubeDownloader.Net6.dll","--docker"]
