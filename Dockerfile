# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Optional: force version when MinVer cannot read tags (e.g. no .git or shallow clone). CI sets this; git-URL docker builds have no .git in context.
ARG MINVERVERSIONOVERRIDE
WORKDIR /src
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && git config --global --add safe.directory /src \
    && rm -rf /var/lib/apt/lists/*
COPY nuget.config .
COPY server/WinDbgSymbolsCachingProxy.csproj server/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "server/WinDbgSymbolsCachingProxy.csproj"
COPY server/ ./server/
# docker build <git-url> omits .git from the context; a local clone includes it. MinVer reads /src/.git when present.
COPY . /tmp/buildctx/
RUN if [ -d /tmp/buildctx/.git ]; then cp -a /tmp/buildctx/.git /src/.git; fi \
    && rm -rf /tmp/buildctx
WORKDIR /src/server
RUN --mount=type=cache,target=/root/.nuget/packages \
    if [ -n "${MINVERVERSIONOVERRIDE}" ]; then export MINVERVERSIONOVERRIDE="${MINVERVERSIONOVERRIDE}"; fi \
    && dotnet publish "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser
WORKDIR /app
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser
ENV HOME=/app
ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
