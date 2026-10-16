FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FefuScheduleBot/FefuScheduleBot.csproj", "FefuScheduleBot/"]
RUN dotnet restore "FefuScheduleBot/FefuScheduleBot.csproj"

COPY . .
WORKDIR "/src/FefuScheduleBot"
RUN dotnet publish "FefuScheduleBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# 1. Добавляем репозитории contrib и non-free для доступа к пакетам шрифтов Microsoft в Debian
RUN echo "deb http://deb.debian.org/debian bookworm contrib non-free" > /etc/apt/sources.list.d/contrib.list \
    && echo "deb http://deb.debian.org/debian bookworm-updates contrib non-free" >> /etc/apt/sources.list.d/contrib.list

# 2. Устанавливаем libgdiplus, fontconfig и пакет шрифтов MS
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    fontconfig \
    libfontconfig1 \
    && echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections \
    && apt-get install -y --no-install-recommends ttf-mscorefonts-installer \
    && rm -rf /var/lib/apt/lists/* \
    && fc-cache -f -v

# 3. Указываем переменные окружения, чтобы FreeSpire и fontconfig гарантированно нашли системные пути шрифтов
ENV LD_LIBRARY_PATH=/usr/share/fonts:/usr/local/share/fonts

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FefuScheduleBot.dll"]