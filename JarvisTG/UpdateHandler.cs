using JarvisTG.Types;
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
using static JarvisTG.Actions.JarvisAction;
using JarvisTG.Actions;
using OpenAI;
using OpenAI.GPT3.Models.RequestModels;
using OpenAI.GPT3.Models;
using OpenAI.GPT3.Models.ResponseModels;
using JarvisTG.Callbacks;
using JarvisTG.Attributes;
using static JarvisTG.Callbacks.JarvisCallback;

namespace JarvisTG
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(UpdateHandler));

        private readonly List<JarvisCommand> commands = new();
        private readonly List<JarvisAction> actions = new();
        public readonly HashSet<string> ActionIds = new();
        private readonly List<JarvisCallback> callbacks = new();
        private const long ownerId = 267376056;
        private const string trainingActionId = "Training";
        internal readonly Dictionary<long, List<ClassificationFile.Item>> LearningChats = new();

        public UpdateHandler()
        {
            ScanAssemblyForCommands();
            ScanAssemblyForActions();
            CollectActionIds();
            ScanAssemblyForCallbacks();
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
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public))
                        {
                            if (method.IsDefined(typeof(TgCommandAttribute), true))
                            {
                                var methodAttributes = method.GetCustomAttributes<TgCommandAttribute>();
                                foreach (TgCommandAttribute methodAttribute in methodAttributes)
                                {
                                    string trigger = methodAttribute.Trigger;
                                    if (!trigger.IsLower())
                                    {
                                        logger.Warn($"The command {trigger} will be ignored because it is not in lowercase.");
                                    }
                                    if (!trigger.StartsWith("/"))
                                    {
                                        logger.Warn($"The command {trigger} will be ignored because it doesn't start with a forward slash.");
                                    }
                                    JarvisCommand command = new()
                                    {
                                        Trigger = trigger,
                                        Execute = method.CreateDelegate<CommandDelegate>(),
                                        PermissionLevel = methodAttribute.PermissionLevel == PermissionLevel.Inherit ? typeAttribute.PermissionLevel : methodAttribute.PermissionLevel,
                                        AllowedChatTypes = methodAttribute.AllowedChatTypes == AllowedChatTypes.Inherit ? typeAttribute.AllowedChatTypes : methodAttribute.AllowedChatTypes
                                    };
                                    commands.Add(command);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ScanAssemblyForActions()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assembly_types = assembly.GetTypes();

                foreach (var type in assembly_types)
                {
                    if (type.IsDefined(typeof(JarvisActionsAttribute), true))
                    {
                        JarvisActionsAttribute typeAttribute = type.GetCustomAttribute<JarvisActionsAttribute>();
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public))
                        {
                            if (method.IsDefined(typeof(JarvisActionAttribute), true))
                            {
                                var methodAttributes = method.GetCustomAttributes<JarvisActionAttribute>();
                                foreach (JarvisActionAttribute methodAttribute in methodAttributes)
                                {
                                    string actionId = methodAttribute.ActionId;
                                    if (actionId.Contains(' '))
                                    {
                                        logger.Warn($"Using spaces in the action ID is not recommended ({actionId}).");
                                    }
                                    if (actionId == trainingActionId && type.Assembly != Assembly.GetAssembly(typeof(JarvisTG)))
                                    {
                                        const string errorMessage = "Defining another use for the Training action is forbidden. This action is used to bypass any other actions if learning.";
                                        ApplicationException exception = new(errorMessage);
                                        logger.Error(errorMessage, exception);
                                        throw exception;
                                    }
                                    if (!actionId.IsCapitalized())
                                    {
                                        logger.Warn("Action Ids will be automatically capitalized (see https://beta.openai.com/docs/api-reference/classifications/create#classifications/create-labels). " +
                                            $"The action {actionId} could be ignored for this reason.");
                                    }
                                    JarvisAction action = new()
                                    {
                                        ActionId = actionId,
                                        Execute = method.CreateDelegate<ActionDelegate>(),
                                        PermissionLevel = methodAttribute.PermissionLevel == PermissionLevel.Inherit ? typeAttribute.PermissionLevel : methodAttribute.PermissionLevel,
                                        AllowedChatTypes = methodAttribute.AllowedChatTypes == AllowedChatTypes.Inherit ? typeAttribute.AllowedChatTypes : methodAttribute.AllowedChatTypes
                                    };
                                    actions.Add(action);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CollectActionIds()
        {
            actions.ForEach(x => ActionIds.Add(x.ActionId));
            ClassificationFile classificationFile = JarvisTG.Config.GetClassificationFile().Result;
            if (classificationFile != null) classificationFile.Items.ForEach(x => ActionIds.Add(x.Label));
            ActionIds.Remove(trainingActionId);
        }

        private void ScanAssemblyForCallbacks()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var assembly_types = assembly.GetTypes();

                foreach (var type in assembly_types)
                {
                    if (type.IsDefined(typeof(TgCallbacksAttribute), true))
                    {
                        TgCallbacksAttribute typeAttribute = type.GetCustomAttribute<TgCallbacksAttribute>();
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public))
                        {
                            if (method.IsDefined(typeof(TgCallbackAttribute), true))
                            {
                                var methodAttributes = method.GetCustomAttributes<TgCallbackAttribute>();
                                foreach (TgCallbackAttribute methodAttribute in methodAttributes)
                                {
                                    string trigger = methodAttribute.Trigger;
                                    if (trigger.Contains('|'))
                                    {
                                        string errorMessage = $"Illegal callback trigger: {trigger}. Callback triggers cannot contain pipe character (|).";
                                        ApplicationException exception = new(errorMessage);
                                        logger.Error(errorMessage, exception);
                                        throw exception;
                                    }
                                    JarvisCallback callback = new()
                                    {
                                        Trigger = trigger,
                                        Execute = method.CreateDelegate<CallbackDelegate>(),
                                        PermissionLevel = methodAttribute.PermissionLevel == PermissionLevel.Inherit ? typeAttribute.PermissionLevel : methodAttribute.PermissionLevel
                                    };
                                    callbacks.Add(callback);
                                }
                            }
                        }
                    }
                }
            }
        }

#if DEBUG
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default)
        {
            logger.Error(exception.ToString());
            throw exception;
        }
#else
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default)
        {
            logger.Error(exception.ToString());
            await botClient.SendTextMessageAsync(ownerId, exception.ToString());
        }
#endif

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageAsync(botClient, update.Message, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data != null)
            {
                string[] args = callbackQuery.Data.Split('|');
                string trigger = args[0];
                bool isApplicable(JarvisCallback x) => x.Trigger == trigger;
                bool permissionDenied = callbacks.Any(isApplicable);
                foreach (JarvisCallback callback in callbacks.Where(isApplicable))
                {
                    if (await UserHasPermission(botClient, callbackQuery, callback.PermissionLevel, cancellationToken))
                    {

                        await callback.Execute(botClient, callbackQuery, args, cancellationToken);
                        permissionDenied = false;
                    }
                }
                if (permissionDenied)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "I'm sorry, but you don't have the authority to do this.", 
                        cancellationToken: cancellationToken);
                }
                if (!callbacks.Any(isApplicable))
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                    logger.Warn($"Unhandled callback query was answered: {callbackQuery.Data}");
                }
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            if (IsBotCommand(message))
            {
                await HandleBotCommand(botClient, message, cancellationToken);
            }
            else if (IsAction(message))
            {
                await HandleAction(botClient, message, cancellationToken);
            }
        }
        private static bool IsBotCommand(Message message)
        {
            static bool commandEntityOffsetZero(MessageEntity x) => x.Type == MessageEntityType.BotCommand && x.Offset == 0;
            return message.Type == MessageType.Text && message.Entities != null && message.Entities.Any(commandEntityOffsetZero);
        }

        private static bool IsAction(Message message)
        {
            bool isMentionOfMe(MessageEntity x) => x.Type == MessageEntityType.Mention && message.Text[x.Offset..].ToLower() == $"@{JarvisTG.BotUser.Username.ToLower()}";
            return message.Type == MessageType.Text &&
                ((message.Entities != null && message.Entities.Any(isMentionOfMe))
                    || message.Text.ToLower().Contains(JarvisTG.JarvisTrigger.ToLower()));
        }


        private async Task HandleBotCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            string trigger = message.OffsetZeroCommandEntityValue().ToLower().TrimEnd($"@{JarvisTG.BotUser.Username.ToLower()}");
            bool isApplicable(JarvisCommand x) => x.Trigger == trigger && message.Chat.Type.IsAllowed(x.AllowedChatTypes);
            bool permissionDenied = commands.Any(isApplicable);
            foreach (JarvisCommand command in commands.Where(isApplicable))
            {
                if (await UserHasPermission(botClient, message, command.PermissionLevel, cancellationToken))
                {
                    await command.Execute(botClient, message, cancellationToken);
                    permissionDenied = false;
                }
            }
            if (permissionDenied)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "I'm sorry, but you don't have the authority to do this.", replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);   // TODO: localization
            }
        }

        private async Task HandleAction(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            string messageText = message.Text;
            string actionId;
            if (LearningChats.ContainsKey(message.Chat.Id))
            {
                actionId = trainingActionId;
            }
            else
            {
                string fileId = await JarvisTG.Config.GetOpenAiFileId(cancellationToken);
                if (fileId == null)
                {
                    logger.Warn("A message was detected, but no OpenAI file ID has been specified. Message will be ignored.");
                    return;
                }
                ClassificationCreateRequest request = new()
                {
                    Model = Engines.Ada,
                    Query = messageText,
                    File = fileId
                };
                ClassificationCreateResponse response = await JarvisTG.OpenAiClient.ClassificationsCreate(request);
                actionId = response.Label;
            }
            bool isApplicable(JarvisAction x) => x.ActionId == actionId && message.Chat.Type.IsAllowed(x.AllowedChatTypes);
            bool permissionDenied = actions.Any(isApplicable);
            foreach (JarvisAction action in actions.Where(isApplicable))
            {
                if (await UserHasPermission(botClient, message, action.PermissionLevel, cancellationToken))
                {
                    await action.Execute(botClient, message, cancellationToken);
                    permissionDenied = false;
                }
            }
            if (permissionDenied)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "I'm sorry, but you don't have the authority to do this.", replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
            }
        }

        private static async Task<bool> UserHasPermission(ITelegramBotClient botClient, Message message, PermissionLevel permissionLevel, CancellationToken cancellationToken = default)
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

        private static async Task<bool> UserHasPermission(ITelegramBotClient botClient, CallbackQuery callbackQuery, PermissionLevel permissionLevel, CancellationToken cancellationToken = default)
        {
            long senderId = callbackQuery.From.Id;
            switch (permissionLevel)
            {
                case PermissionLevel.Owner:
                    return senderId == ownerId;
                case PermissionLevel.GroupAdmin:
                    if (callbackQuery.Message == null || !callbackQuery.Message.Chat.IsGroup())
                        return true;
                    ChatMember[] administrators = await botClient.GetChatAdministratorsAsync(callbackQuery.Message.Chat.Id, cancellationToken: cancellationToken);
                    return administrators.Any(x => x.User.Id == senderId);
                default:
                    return true;
            }
        }
    }
}
