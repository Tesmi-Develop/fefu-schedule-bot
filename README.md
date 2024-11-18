# 📒 Fefu Schedule 📒

This telegram bot was created for FEFU university students. It allows you to get the university schedule for a group

# 📦 Installation 📦
1) clone this repository

```bash
git clone https://github.com/Tesmi-Develop/fefu-schedule-bot
```

2) install the package separately

```bash
cd FefuScheduleBot/

dotnet nuget add source https://pkgs.dev.azure.com/tgbots/Telegram.Bot/_packaging/release/nuget/v3/index.json -n Telegram.Bot
```

# ⚒️ Running ⚒️
1) Сall the build program

```bash
dotnet build --configuration=Release
```

2) Run program (call this command inside the directory ``FefuScheduleBot``)

```bash
dotnet run
```

3) The bot will automatically stop and ask you to fill in the .env file. The file will be created at the path ``FefuScheduleBot/bin/Release/net8.0/.env``

4) Run the bot again