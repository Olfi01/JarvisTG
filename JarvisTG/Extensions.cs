using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JarvisTG
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the value of the command entity at offset 0. Will throw an <see cref="InvalidOperationException"/> if none is found.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to get the entity value of.</param>
        /// <returns>The string command</returns>
        public static string OffsetZeroCommandEntityValue(this Message message)
        {
            if (message.Type != MessageType.Text)
            {
                throw new ArgumentException("Can only find command entity value of text messages", nameof(message));
            }
            for (int i = 0; i < message.Entities.Length; i++)
            {
                MessageEntity messageEntity = message.Entities[i];
                if (messageEntity.Offset == 0 && messageEntity.Type == MessageEntityType.BotCommand)
                {
                    return message.EntityValues.ElementAt(i);
                }
            }
            throw new InvalidOperationException("Couldn't find a command entity with offset zero.");
        }

        public static string TrimEnd(this string str, string trim) => str.EndsWith(trim) ? str.Remove(str.LastIndexOf(trim)) : str;

        public static bool IsGroup(this Chat chat) => chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup;

        public static bool IsCapitalized(this string str) => str.Length > 1 ? str == str.ToUpper()[..1] + str.ToLower()[1..] : str == str.ToUpper();

        public static bool IsLower(this string str) => str == str.ToLower();

        public static bool IsAllowed(this ChatType chatType, AllowedChatTypes allowed)
        {
            return chatType switch
            {
                ChatType.Private => allowed.HasFlag(AllowedChatTypes.Private),
                ChatType.Group => allowed.HasFlag(AllowedChatTypes.Group),
                ChatType.Channel => allowed.HasFlag(AllowedChatTypes.Channel),
                ChatType.Supergroup => allowed.HasFlag(AllowedChatTypes.Supergroup),
                ChatType.Sender => allowed.HasFlag(AllowedChatTypes.Sender),
                _ => allowed.HasFlag(AllowedChatTypes.Any)
            };
        }
    }
}
