using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramWordBot;

static class MenuManager
{
    public static async Task ShowMainMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Создать новый документ", "Выбрать документ" },
            new KeyboardButton[] { "Скачать" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public static async Task ShowDocumentMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Создать новый документ", "Выбрать документ" },
            new KeyboardButton[] { "Главное меню" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Документ создан. Выберите действие:", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public static async Task ShowDocumentList(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        if (DocumentManager.UserDocuments.ContainsKey(chatId) && DocumentManager.UserDocuments[chatId].Count > 0)
        {
            var buttons = DocumentManager.UserDocuments[chatId].Select(doc => new KeyboardButton($"Документ: {doc}")).ToArray();

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                buttons, 
                new KeyboardButton[] { "Главное меню" }
            }) { ResizeKeyboard = true };
            await botClient.SendTextMessageAsync(chatId, "Выберите документ:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "У вас пока нет созданных документов.", cancellationToken: cancellationToken);
        }
    }

    public static async Task EditDocumentMenu(ITelegramBotClient botClient, long chatId, string documentName, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, $"Введите текст для колонок (разделяйте |):\nИзменить документ: {documentName} | Колонка 1 | Колонка 2 | Колонка 3", cancellationToken: cancellationToken);
    }

    public static async Task ShowDownloadMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        if (DocumentManager.UserDocuments.ContainsKey(chatId) && DocumentManager.UserDocuments[chatId].Count > 0)
        {
            var buttons = DocumentManager.UserDocuments[chatId].Select(doc => new KeyboardButton($"Скачать: {doc}")).ToArray();

            var keyboard = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
            await botClient.SendTextMessageAsync(chatId, "Выберите документ для скачивания:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "У вас пока нет созданных документов.", cancellationToken: cancellationToken);
        }
    }
}