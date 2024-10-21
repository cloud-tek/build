FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CloudTek.Build.Tool/CloudTek.Build.Tool.csproj", "src/CloudTek.Build.Tool/"]
RUN dotnet restore "src/CloudTek.Build.Tool/CloudTek.Build.Tool.csproj"
COPY . .
WORKDIR "/src/src/CloudTek.Build.Tool"
RUN dotnet build "CloudTek.Build.Tool.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CloudTek.Build.Tool.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudTek.Build.Tool.dll"]
