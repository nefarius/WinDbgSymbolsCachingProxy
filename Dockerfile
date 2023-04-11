FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/WinDbgSymbolsCachingProxy.csproj", "src/"]
RUN dotnet restore "src/WinDbgSymbolsCachingProxy.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinDbgSymbolsCachingProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV DEBIAN_FRONTEND noninteractive
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update
RUN apt-get install -y fontconfig    
RUN apt-get install -y ttf-mscorefonts-installer
RUN apt-get install -y libfreetype6
RUN apt-get install -y libfontconfig1
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinDbgSymbolsCachingProxy.dll"]
