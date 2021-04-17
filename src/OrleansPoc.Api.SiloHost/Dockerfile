#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OrleansPoc.Api.SiloHost/OrleansPoc.Api.SiloHost.csproj", "OrleansPoc.Api.SiloHost/"]
COPY ["OrleansPoc.Entities/OrleansPoc.Entities.csproj", "OrleansPoc.Entities/"]
COPY ["OrleansPoc.Api.Interfaces/OrleansPoc.Api.Interfaces.csproj", "OrleansPoc.Api.Interfaces/"]
COPY ["OrleansPoc.Models/OrleansPoc.Models.csproj", "OrleansPoc.Models/"]
COPY ["OrleansPoc.Api.Grains/OrleansPoc.Api.Grains.csproj", "OrleansPoc.Api.Grains/"]
RUN dotnet restore "OrleansPoc.Api.SiloHost/OrleansPoc.Api.SiloHost.csproj"
COPY . .
WORKDIR "/src/OrleansPoc.Api.SiloHost"
RUN dotnet build "OrleansPoc.Api.SiloHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrleansPoc.Api.SiloHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrleansPoc.Api.SiloHost.dll"]