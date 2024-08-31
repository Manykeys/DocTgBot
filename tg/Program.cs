using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace TelegramWordBot;

class Program
{
    private static string BotToken;

    static async Task Main(string[] args)
    {
        Env.Load();
        BotToken = Environment.GetEnvironmentVariable("token");

        var botClient = new TelegramBotClient(BotToken);

        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        botClient.StartReceiving(
            updateHandler: BotHandler.HandleUpdateAsync,
            pollingErrorHandler: BotHandler.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Бот запущен. Нажмите любую клавишу для остановки.");
        Console.ReadKey();
            
        cts.Cancel();
    }
}