#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OrleansPoc.Http.Producer/OrleansPoc.Http.Producer.csproj", "OrleansPoc.Http.Producer/"]
COPY ["OrleansPoc.Models/OrleansPoc.Models.csproj", "OrleansPoc.Models/"]
RUN dotnet restore "OrleansPoc.Http.Producer/OrleansPoc.Http.Producer.csproj"
COPY . .
WORKDIR "/src/OrleansPoc.Http.Producer"
RUN dotnet build "OrleansPoc.Http.Producer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrleansPoc.Http.Producer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrleansPoc.Http.Producer.dll"]