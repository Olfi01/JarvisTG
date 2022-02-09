using JarvisTG.Types;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Filter;
using log4net.Layout;
using OpenAI;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using System;
using System.IO;
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

        public static JarvisConfig Config { get; } = new JarvisConfig(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JarvisTG"));

        public const string JarvisTrigger = "jarvis";
        private const string openAiOrganization = "CrazyPokemonDev";

        private static string tgBotApiToken;
        private static string openAiToken;

        private static TelegramBotClient tgClient;
        public static User BotUser;
        private static readonly ReceiverOptions tgReceiverOptions = new()
        {
            AllowedUpdates = new UpdateType[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            },
            ThrowPendingUpdates = true
        };
        public static readonly CancellationTokenSource CancellationTokenSource = new();
        public static readonly UpdateHandler UpdateHandler = new();

        public static OpenAIService OpenAiClient;

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            if (args.Length < 2)
            {
                Console.WriteLine("Syntax: JarvisTG [tg-token] [openai-token]");
                return;
            }
            tgBotApiToken = args[0];
            tgClient = new TelegramBotClient(tgBotApiToken);
            openAiToken = args[1];
            OpenAiClient = new OpenAIService(new OpenAiOptions { ApiKey = openAiToken, Organization = openAiOrganization });

            BotUser = tgClient.GetMeAsync().Result;
            logger.Info($"Telegram User: {BotUser}");

            try
            {
                tgClient.ReceiveAsync(UpdateHandler, tgReceiverOptions, CancellationTokenSource.Token).Wait();
            }
            catch (OperationCanceledException)
            {
                logger.Info("Receiving was cancelled, shutting down.");
            }
        }
    }
}
