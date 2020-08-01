#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["GearClient/Server/GearClient.Server.csproj", "GearClient/Server/"]
COPY ["GearClient/Client/GearClient.Client.csproj", "GearClient/Client/"]
COPY ["GearClient/Shared/GearClient.Shared.csproj", "GearClient/Shared/"]
COPY ["Plotter/Plotter.csproj", "Plotter/"]
COPY ["InvoluteGears/InvoluteGears.csproj", "InvoluteGears/"]
RUN dotnet restore "GearClient/Server/GearClient.Server.csproj"
COPY . .
WORKDIR "/src/GearClient/Server"
RUN dotnet build "GearClient.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GearClient.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GearClient.Server.dll"]