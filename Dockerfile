FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./nuget.config .
COPY ["src/WinDbgSymbolsCachingProxy.csproj", "src/"]
RUN dotnet restore "src/WinDbgSymbolsCachingProxy.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV DEBIAN_FRONTEND noninteractive
RUN echo "deb http://deb.debian.org/debian/ bookworm main contrib non-free-firmware" > /etc/apt/sources.list.d/contrib.list
RUN apt update && \
    apt install software-properties-common -y && \
    apt-add-repository contrib non-free -y && \
    apt update && \
    apt install -y fontconfig && \    
    apt install -y ttf-mscorefonts-installer && \
    apt install -y libfreetype6 && \
    apt install -y libfontconfig1
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
