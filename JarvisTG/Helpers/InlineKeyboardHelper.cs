using JarvisTG.Callbacks.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisTG.Helpers
{
    internal class InlineKeyboardHelper
    {
        private const int buttonsPerPage = 8;
        private const int buttonsPerRow = 2;

        private static readonly Dictionary<Guid, string> decoding = new();
        private static readonly Dictionary<string, Guid> encoding = new();

        internal static InlineKeyboardMarkup CreateActionIdKeyboard(string messageText, IEnumerable<string> actionIds, int page = 0)
        {
            int actionIdsCount = actionIds.Count();
            if (actionIdsCount == 0) return null;
            if (actionIdsCount < page * buttonsPerPage + 1) page = 0;

            List<List<InlineKeyboardButton>> buttons = new();
            List<InlineKeyboardButton> row = new();
            for (int i = page * buttonsPerPage; i < Math.Min((page + 1) * buttonsPerPage, actionIdsCount); i++)
            {
                string actionId = actionIds.ElementAt(i);
                InlineKeyboardButton button = new(actionId) { CallbackData = TrainingCallbacks.TrainingCallback + "|" + actionId + "|" + CallbackEncode(messageText) };
                if (row.Count >= buttonsPerRow)
                {
                    buttons.Add(row);
                    row = new();
                }
                row.Add(button);
            }
            buttons.Add(row);
            row = new();

            if (page > 0)
            {
                row.Add(new InlineKeyboardButton("<-") { CallbackData = TrainingCallbacks.PageCallback + "|" + CallbackEncode(messageText) + "|" + (page - 1) });
            }
            if ((page + 1) * buttonsPerPage < actionIdsCount)
            {
                row.Add(new InlineKeyboardButton("->") { CallbackData = TrainingCallbacks.PageCallback + "|" + CallbackEncode(messageText) + "|" + (page + 1) });
            }
            if (row.Count > 0) buttons.Add(row);

            return new InlineKeyboardMarkup(buttons);
        }

        public static string CallbackEncode(string str) 
        {
            if (encoding.ContainsKey(str))
            {
                return encoding[str].ToString("N");
            }
            else
            {
                Guid guid = Guid.NewGuid();
                decoding.Add(guid, str);
                encoding.Add(str, guid);
                return guid.ToString("N");
            }
        }

        public static string CallbackDecode(string str)
        {
            Guid guid = Guid.Parse(str);
            if (decoding.ContainsKey(guid))
            {
                return decoding[guid];
            }
            else return string.Empty;
        }
    }
}
