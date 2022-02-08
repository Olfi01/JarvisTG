using log4net;
using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JarvisTG
{
    class JarvisTG
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(JarvisTG));

        private static string tgBotApiToken;
        private static string openAiToken;

        private static TelegramBotClient tgClient;
        public static User BotUser;
        private static readonly ReceiverOptions tgReceiverOptions = new()
        {
            AllowedUpdates = new UpdateType[]
            {
                UpdateType.Message
            },
            ThrowPendingUpdates = true
        };
        public static readonly CancellationTokenSource CancellationTokenSource = new();

        static async void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Syntax: JarvisTG [tg-token] [openai-token]");
                return;
            }
            tgBotApiToken = args[0];
            openAiToken = args[1];

            tgClient = new TelegramBotClient(tgBotApiToken);

            BotUser = await tgClient.GetMeAsync();
            logger.Info($"Telegram User: {BotUser}");

            try
            {
                await tgClient.ReceiveAsync<UpdateHandler>(tgReceiverOptions, CancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                logger.Info("Receiving was cancelled, shutting down.");
            }
        }
    }
}
