using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramWordBot;

static class BotHandler
{
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
                
            switch (messageText)
            {
                case "/start":
                case "Главное меню":
                    await MenuManager.ShowMainMenu(botClient, chatId, cancellationToken);
                    break;
                    
                case "Создать новый документ":
                    var filePath = DocumentManager.CreateNewDocument(chatId);
                    await botClient.SendTextMessageAsync(chatId, "Документ создан!", cancellationToken: cancellationToken);
                    await MenuManager.ShowDocumentMenu(botClient, chatId, cancellationToken);
                    break;

                case "Выбрать документ":
                    await MenuManager.ShowDocumentList(botClient, chatId, cancellationToken);
                    break;

                case "Скачать":
                    await MenuManager.ShowDownloadMenu(botClient, chatId, cancellationToken);
                    break;

                default:
                    await DocumentManager.ProcessDocumentCommands(botClient, chatId, messageText, cancellationToken);
                    break;
            }
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}