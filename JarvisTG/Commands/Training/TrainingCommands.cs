using JarvisTG.Types;
using OpenAI.GPT3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace JarvisTG.Commands.Training
{
    [TgCommands(PermissionLevel = PermissionLevel.Owner)]
    public static class TrainingCommands
    {
        [TgCommand("/stop")]
        public static async Task Stop(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Stopping.", replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
            JarvisTG.CancellationTokenSource.Cancel();
        }

        [TgCommand("/startlearning", AllowedChatTypes = AllowedChatTypes.Private)]
        public static async Task StartLearning(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            long chatId = message.Chat.Id;
            if (JarvisTG.UpdateHandler.LearningChats.ContainsKey(chatId)) return;
            await botClient.SendTextMessageAsync(chatId, "I will start learning in this chat now.", replyToMessageId: message.MessageId, 
                cancellationToken: cancellationToken);
            JarvisTG.UpdateHandler.LearningChats.Add(chatId, new List<ClassificationFile.Item>());
        }

        [TgCommand("/stoplearning", AllowedChatTypes = AllowedChatTypes.Any)]
        public static async Task StopLearning(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            long chatId = message.Chat.Id;
            if (!JarvisTG.UpdateHandler.LearningChats.ContainsKey(chatId)) return;
            await botClient.SendTextMessageAsync(chatId, "I will stop learning in this chat now.", replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
            ClassificationFile classificationFile = await JarvisTG.Config.GetClassificationFile(cancellationToken);
            if (classificationFile == null) classificationFile = new();
            classificationFile.Items.AddRange(JarvisTG.UpdateHandler.LearningChats[chatId]);
            if (classificationFile.Items.Count <= 0) return;
            string tempFilePath = $"{chatId}.jsonl";
            await classificationFile.WriteToPath(tempFilePath, cancellationToken);
            using var stream = File.OpenRead(tempFilePath);
            InputOnlineFile file = new(stream)
            {
                FileName = $"{chatId}.jsonl"
            };
            await botClient.SendDocumentAsync(chatId, file, cancellationToken: cancellationToken);
        }

        [TgCommand("/addaction")]
        public static async Task AddActionId(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
        {
            string[] split = message.Text.Split(' ');
            if (split.Length < 2) return;
            string newActionId = split[1];
            if (newActionId.Contains('|')) return;
            JarvisTG.UpdateHandler.ActionIds.Add(newActionId);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Added {newActionId} as new action ID for training.", 
                replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
        }

        // TODO: upload file to OpenAI: JarvisTG.OpenAiClient.Files.FileUpload(UploadFilePurposes.Classifications, stream, fileName);
    }
}
