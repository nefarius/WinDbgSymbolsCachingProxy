# syntax=docker/dockerfile:1
# Build from the repository root:
#   docker build -f agent/Dockerfile -t wdscp-harvesting-agent .

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
EXPOSE 5088

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
# Optional: force version when MinVer cannot read tags (e.g. no .git or shallow clone). CI sets this; git-URL docker builds have no .git in context.
ARG MINVERVERSIONOVERRIDE
WORKDIR /src
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && git config --global --add safe.directory /src \
    && rm -rf /var/lib/apt/lists/*
COPY nuget.config global.json Directory.Build.props Directory.Packages.props ./
COPY agent/HarvestingAgent.csproj agent/
COPY shared/WinDbgSymbolsCachingProxy.UpdateCheck/WinDbgSymbolsCachingProxy.UpdateCheck.csproj shared/WinDbgSymbolsCachingProxy.UpdateCheck/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "agent/HarvestingAgent.csproj"
COPY agent/ ./agent/
COPY shared/WinDbgSymbolsCachingProxy.UpdateCheck/ ./shared/WinDbgSymbolsCachingProxy.UpdateCheck/
# docker build <git-url> omits .git from the context; a local clone includes it. MinVer reads /src/.git when present.
COPY . /tmp/buildctx/
RUN if [ -d /tmp/buildctx/.git ]; then cp -a /tmp/buildctx/.git /src/.git; fi \
    && rm -rf /tmp/buildctx
WORKDIR /src/agent
# Single-file publishing is a Windows-release setting and needs a RID; the container runs the framework-dependent DLL.
RUN --mount=type=cache,target=/root/.nuget/packages \
    if [ -n "${MINVERVERSIONOVERRIDE}" ]; then export MINVERVERSIONOVERRIDE="${MINVERVERSIONOVERRIDE}"; fi \
    && dotnet publish "HarvestingAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:PublishSingleFile=false

FROM base AS final
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser \
    && mkdir -p /app/logs /data /symbols \
    && chown appuser:appgroup /app /app/logs /data
WORKDIR /app
COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser
ENV HOME=/app
# Settings live under $XDG_DATA_HOME/WinDbgSymbolsCachingProxy/HarvestingAgent/agent-settings.json (mount /data to persist).
ENV XDG_DATA_HOME=/data
# The UI has no authentication on Linux: only publish this port on the host loopback (-p 127.0.0.1:5088:5088).
ENV ASPNETCORE_URLS=http://+:5088
VOLUME ["/data", "/app/logs"]
ENTRYPOINT ["dotnet", "HarvestingAgent.dll"]
