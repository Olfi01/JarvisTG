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
using JarvisTG.Helpers;

namespace JarvisTG.Actions.Training
{
    [JarvisActions]
    public static class TrainingActions
    {
        [JarvisAction("Training")]
        public static async Task Training(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            InlineKeyboardMarkup inlineKeyboard = InlineKeyboardHelper.CreateActionIdKeyboard(message.Text, JarvisTG.UpdateHandler.ActionIds, page: 0);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Which action ID should i assign to this example?", replyToMessageId: message.MessageId, 
                replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }
    }
}
