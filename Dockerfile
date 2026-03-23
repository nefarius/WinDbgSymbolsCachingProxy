# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG MINVERVERSIONOVERRIDE=0.0.0-local
ENV MINVERVERSIONOVERRIDE=${MINVERVERSIONOVERRIDE}
WORKDIR /src
COPY nuget.config .
COPY server/WinDbgSymbolsCachingProxy.csproj server/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "server/WinDbgSymbolsCachingProxy.csproj"
COPY server/ ./server/
WORKDIR /src/server
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV DEBIAN_FRONTEND=noninteractive
RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    --mount=type=cache,target=/var/lib/apt/lists,sharing=locked \
    set -eux; \
    echo "deb http://deb.debian.org/debian/ bookworm main contrib non-free-firmware" > /etc/apt/sources.list.d/contrib.list; \
    apt-get update; \
    apt-get install -y --no-install-recommends software-properties-common; \
    apt-add-repository contrib non-free -y; \
    apt-get update; \
    echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections; \
    apt-get install -y --no-install-recommends \
        fontconfig \
        ttf-mscorefonts-installer \
        libfreetype6 \
        libfontconfig1 \
        curl \
    ; \
    curl -sSL "https://github.com/fullstorydev/grpcurl/releases/download/v1.9.2/grpcurl_1.9.2_linux_x86_64.tar.gz" | tar -xz -C /usr/local/bin; \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
