#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OrleansPoc.Processor.SiloHost/OrleansPoc.Processor.SiloHost.csproj", "OrleansPoc.Processor.SiloHost/"]
COPY ["OrleansPoc.Processor.Grains/OrleansPoc.Processor.Grains.csproj", "OrleansPoc.Processor.Grains/"]
COPY ["OrleansPoc.Abstractions/OrleansPoc.Abstractions.csproj", "OrleansPoc.Abstractions/"]
COPY ["OrleansPoc.Processor.Interfaces/OrleansPoc.Processor.Interfaces.csproj", "OrleansPoc.Processor.Interfaces/"]
RUN dotnet restore "OrleansPoc.Processor.SiloHost/OrleansPoc.Processor.SiloHost.csproj"
COPY . .
WORKDIR "/src/OrleansPoc.Processor.SiloHost"
RUN dotnet build "OrleansPoc.Processor.SiloHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrleansPoc.Processor.SiloHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrleansPoc.Processor.SiloHost.dll"]