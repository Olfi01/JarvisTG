using JarvisTG.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JarvisTG.Commands
{
    public class JarvisCommand
    {
        public delegate Task CommandDelegate(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);

        public string Trigger { get; init; }

        public CommandDelegate Execute { get; init; }

        public PermissionLevel PermissionLevel { get; init; }
    }
}
