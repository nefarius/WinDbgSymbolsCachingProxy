# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
# Optional: force version when MinVer cannot read tags (e.g. no .git or shallow clone). CI sets this; git-URL docker builds have no .git in context.
ARG MINVERVERSIONOVERRIDE
WORKDIR /src
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && git config --global --add safe.directory /src \
    && rm -rf /var/lib/apt/lists/*
COPY nuget.config global.json Directory.Build.props Directory.Packages.props ./
COPY server/WinDbgSymbolsCachingProxy.csproj server/
COPY shared/WinDbgSymbolsCachingProxy.UpdateCheck/WinDbgSymbolsCachingProxy.UpdateCheck.csproj shared/WinDbgSymbolsCachingProxy.UpdateCheck/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "server/WinDbgSymbolsCachingProxy.csproj"
COPY server/ ./server/
COPY shared/WinDbgSymbolsCachingProxy.UpdateCheck/ ./shared/WinDbgSymbolsCachingProxy.UpdateCheck/
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
    # ttf-mscorefonts-installer comes from Ubuntu's multiverse component, which the base image enables.
    apt-get update; \
    echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections; \
    apt-get install -y --no-install-recommends \
        fontconfig \
        ttf-mscorefonts-installer \
        libfreetype6 \
        libfontconfig1 \
        curl \
    ; \
    rm -rf /var/lib/apt/lists/*
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser \
    && mkdir -p /app/logs /data/keys \
    && chown appuser:appgroup /app /app/logs /data /data/keys
WORKDIR /app
COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser
ENV HOME=/app
# Persist Data Protection keys (auth cookies) on a mountable volume instead of the container layer.
ENV ServiceConfig__DataProtectionKeysPath=/data/keys
VOLUME ["/data/keys", "/app/logs"]
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -fs -o /dev/null --max-time 4 http://localhost:8080/info || exit 1
ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
