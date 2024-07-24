FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 1111

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ollim.Bot/Ollim.Bot.csproj", "Ollim.Bot/"]
COPY ["Ollim.Infrastructure/Ollim.Infrastructure.csproj", "Ollim.Infrastructure/"]
RUN dotnet restore "./Ollim.Bot/Ollim.Bot.csproj"
COPY . .
WORKDIR "/src/Ollim.Bot"
RUN dotnet build "./Ollim.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ollim.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ollim.Bot.dll"]