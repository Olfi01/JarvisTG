using JarvisTG.Attributes;
using JarvisTG.Commands;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static JarvisTG.Commands.JarvisCommand;

namespace JarvisTG
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(UpdateHandler));

        private readonly List<JarvisCommand> commands = new();
        private const long ownerId = 267376056;

        public UpdateHandler()
        {
            ScanAssemblyForCommands();
        }

        private void ScanAssemblyForCommands()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assembly_types = assembly.GetTypes();

                foreach (var type in assembly_types)
                {
                    if (type.IsDefined(typeof(TgCommandsAttribute), true))
                    {
                        TgCommandsAttribute typeAttribute = type.GetCustomAttribute<TgCommandsAttribute>();
                        foreach (var method in type.GetMethods())
                        {
                            if (method.IsDefined(typeof(TgCommandAttribute), true))
                            {
                                TgCommandAttribute methodAttribute = method.GetCustomAttribute<TgCommandAttribute>();
                                JarvisCommand command = new()
                                {
                                    Trigger = methodAttribute.Trigger,
                                    Execute = method.CreateDelegate<CommandDelegate>(),
                                    PermissionLevel = methodAttribute.PermissionLevel == PermissionLevel.Inherit ? typeAttribute.PermissionLevel : methodAttribute.PermissionLevel
                                };
                                commands.Add(command);
                            }
                        }
                    }
                }
            }
        }

#if DEBUG
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logger.Error(exception.ToString());
            throw exception;
        }
#else
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logger.Error(exception.ToString());
            await botClient.SendTextMessageAsync(ownerId, exception.ToString());
        }
#endif

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageAsync(botClient, update.Message, cancellationToken);
                    break;
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (IsBotCommand(message))
            {
                await HandleBotCommand(botClient, message, cancellationToken);
            }
        }

        private static bool IsBotCommand(Message message)
        {
            static bool commandEntityOffsetZero(MessageEntity x) => x.Type == MessageEntityType.BotCommand && x.Offset == 0;
            return message.Type == MessageType.Text && message.Entities.Any(commandEntityOffsetZero);
        }

        private async Task HandleBotCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            string messageText = message.Text;
            string commandString = message.OffsetZeroCommandEntityValue().TrimEnd($"@{JarvisTG.BotUser.Username}");
            foreach (JarvisCommand command in commands.Where(x => x.Trigger == commandString))
            {
                if (await UserHasPermission(botClient, message, command.PermissionLevel, cancellationToken))
                {
                    await command.Execute(botClient, message, cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "I'm sorry, but you don't have the authority to do this.", replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);   // TODO: localization
                }
            }
        }

        private static async Task<bool> UserHasPermission(ITelegramBotClient botClient, Message message, PermissionLevel permissionLevel, CancellationToken cancellationToken)
        {
            switch (permissionLevel)
            {
                case PermissionLevel.Owner:
                    return message.From.Id == ownerId;
                case PermissionLevel.GroupAdmin:
                    if (!message.Chat.IsGroup())
                        return true;
                    ChatMember[] administrators = await botClient.GetChatAdministratorsAsync(message.Chat.Id, cancellationToken: cancellationToken);
                    return administrators.Any(x => x.User.Id == message.From.Id);
                default:
                    return true;
            }
        }
    }
}
