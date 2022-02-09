using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JarvisTG.Callbacks
{
    public class JarvisCallback
    {
        public delegate Task CallbackDelegate(ITelegramBotClient client, CallbackQuery query, string[] args, CancellationToken cancellationToken);
        public string Trigger { get; init; }
        public PermissionLevel PermissionLevel { get; init; } = PermissionLevel.Inherit;
        public CallbackDelegate Execute { get; init; }
    }
}
