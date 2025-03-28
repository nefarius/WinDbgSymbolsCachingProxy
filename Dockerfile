﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./nuget.config .
COPY ["server/WinDbgSymbolsCachingProxy.csproj", "server/"]
RUN dotnet restore "server/WinDbgSymbolsCachingProxy.csproj"
COPY . .
WORKDIR "/src/server"
RUN dotnet build "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV DEBIAN_FRONTEND=noninteractive
RUN echo "deb http://deb.debian.org/debian/ bookworm main contrib non-free-firmware" > /etc/apt/sources.list.d/contrib.list
RUN apt update && \
    apt install software-properties-common -y && \
    apt-add-repository contrib non-free -y && \
    apt update && \
    apt install -y fontconfig && \
    apt install -y ttf-mscorefonts-installer && \
    apt install -y libfreetype6 && \
    apt install -y libfontconfig1
RUN apt install curl -y && \
    curl -sSL "https://github.com/fullstorydev/grpcurl/releases/download/v1.9.2/grpcurl_1.9.2_linux_x86_64.tar.gz" | tar -xz -C /usr/local/bin
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
