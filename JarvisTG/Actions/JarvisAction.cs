using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JarvisTG.Actions
{
    public class JarvisAction
    {
        public delegate Task ActionDelegate(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);

        public string ActionId { get; init; }

        public ActionDelegate Execute { get; init; }

        public PermissionLevel PermissionLevel { get; init; }

        public AllowedChatTypes AllowedChatTypes { get; init; }
    }
}
