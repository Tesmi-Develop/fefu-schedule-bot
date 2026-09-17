FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FefuScheduleBot/FefuScheduleBot.csproj", "FefuScheduleBot/"]
RUN dotnet restore "FefuScheduleBot/FefuScheduleBot.csproj"

COPY . .
WORKDIR "/src/FefuScheduleBot"
RUN dotnet publish "FefuScheduleBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    fontconfig \
    libfontconfig1 \
    cabextract \
    xfonts-utils \
    && sed -i 's/main/main contrib non-free/g' /etc/apt/sources.list.d/debian.sources \
    && apt-get update \
    && echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections \
    && apt-get install -y --no-install-recommends ttf-mscorefonts-installer \
    && fc-cache -f -v \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FefuScheduleBot.dll"]