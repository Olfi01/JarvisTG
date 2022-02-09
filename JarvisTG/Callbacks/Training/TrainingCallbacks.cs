using JarvisTG.Attributes;
using JarvisTG.Helpers;
using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisTG.Callbacks.Training
{
    [TgCallbacks(PermissionLevel = Types.PermissionLevel.Owner)]
    public static class TrainingCallbacks
    {
        public const string PageCallback = "Page";
        public const string TrainingCallback = "Training";
        private const string TrainingSource = "JarvisTGTraining";

        [TgCallback(PageCallback)]
        public static async Task Page(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] args, CancellationToken cancellationToken = default)
        {
            if (callbackQuery.Message == null)
            {
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }
            InlineKeyboardMarkup replyMarkup;
            if (args.Length < 3 || !int.TryParse(args[2], out int page))
            {
                replyMarkup = null;
            }
            else
            {
                replyMarkup = InlineKeyboardHelper.CreateActionIdKeyboard(args[1], JarvisTG.UpdateHandler.ActionIds, page);
            }
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            await botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                replyMarkup, cancellationToken: cancellationToken);
        }

        [TgCallback(TrainingCallback)]
        public static async Task Training(ITelegramBotClient botClient, CallbackQuery callbackQuery, string[] args, CancellationToken cancellationToken = default)
        {
            Message message = callbackQuery.Message;
            if (message == null)
            {
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            long chatId = message.Chat.Id;
            Dictionary<long, List<ClassificationFile.Item>> learningChats = JarvisTG.UpdateHandler.LearningChats;
            if (args.Length < 3 || !learningChats.ContainsKey(chatId))
            {
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
                await botClient.EditMessageReplyMarkupAsync(chatId, message.MessageId,
                    null, cancellationToken: cancellationToken);
                return;
            }

            string actionId = args[1];
            string messageText = InlineKeyboardHelper.CallbackDecode(args[2]);
            learningChats[chatId].Add(new ClassificationFile.Item(messageText, actionId,
                new ClassificationFile.Metadata
                {
                    Source = TrainingSource,
                    TelegramUserId = callbackQuery.From.Id
                }));
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Thanks!", cancellationToken: cancellationToken);
            await botClient.EditMessageTextAsync(chatId, message.MessageId, $"This message was classified as {actionId}.",
                    replyMarkup: null, cancellationToken: cancellationToken);
        }
    }
}
