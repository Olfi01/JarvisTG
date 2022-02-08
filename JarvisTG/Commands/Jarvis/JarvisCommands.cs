using JarvisTG.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JarvisTG.Commands.Jarvis
{
    [TgCommands(PermissionLevel = PermissionLevel.Owner)]
    public static class JarvisCommands
    {
        [TgCommand("/stop")]
        public static async Task Stop(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Stopping.", replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
            JarvisTG.CancellationTokenSource.Cancel();
        }
    }
}
