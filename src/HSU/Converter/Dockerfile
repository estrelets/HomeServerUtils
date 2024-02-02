FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Converter/Converter.csproj", "Converter/"]
RUN dotnet restore "Converter/Converter.csproj"
COPY . .
WORKDIR "/src/Converter"
RUN dotnet build "Converter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Converter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN apt update
RUN apt install -y wget xz-utils
RUN wget https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz
RUN tar -xf ffmpeg-release-amd64-static.tar.xz
ENV FFMPEG_PATH=/app/ffmpeg-6.1-amd64-static/ffmpeg

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Converter.dll"]
