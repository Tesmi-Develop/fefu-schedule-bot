FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FefuScheduleBot/FefuScheduleBot.csproj", "FefuScheduleBot/"]
RUN dotnet restore "FefuScheduleBot/FefuScheduleBot.csproj"

COPY . .
WORKDIR "/src/FefuScheduleBot"
RUN dotnet publish "FefuScheduleBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Устанавливаем графические библиотеки
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    fontconfig \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Создаем директорию для шрифтов
RUN mkdir -p /usr/share/fonts/truetype/custom

# Копируем шрифты
COPY FefuScheduleBot/Fonts/ /usr/share/fonts/truetype/custom/

# Создаем явную конфигурацию fontconfig, чтобы система гарантированно просканировала папку
RUN echo '<?xml version="1.0"?><fontconfig><dir>/usr/share/fonts/truetype/custom</dir></fontconfig>' > /etc/fonts/local.conf

# Обновляем кэш шрифтов
RUN fc-cache -f -v

ENV LD_LIBRARY_PATH=/usr/share/fonts:/usr/local/share/fonts

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FefuScheduleBot.dll"]