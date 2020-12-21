#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Highway.Samples.Console/Highway.Samples.Console.csproj", "Highway.Samples.Console/"]
COPY ["Highway.Persistence.InMemory/Highway.Persistence.InMemory.csproj", "Highway.Persistence.InMemory/"]
COPY ["Highway.Core/Highway.Core.csproj", "Highway.Core/"]
RUN dotnet restore "Highway.Samples.Console/Highway.Samples.Console.csproj"
COPY . .
WORKDIR "/src/Highway.Samples.Console"
RUN dotnet build "Highway.Samples.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Highway.Samples.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Highway.Samples.Console.dll"]