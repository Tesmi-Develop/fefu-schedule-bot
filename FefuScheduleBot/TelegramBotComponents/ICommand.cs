using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents;

public interface ICommand
{
    Task Execute(Message message, DependenciesContainer container);
}