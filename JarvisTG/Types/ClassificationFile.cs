using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    public class ClassificationFile
    {
#nullable enable
        [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Metadata
        {
            [JsonProperty(PropertyName = "source")]
            public string? Source { get; set; }
            [JsonProperty(PropertyName = "telegramUserId")]
            public long TelegramUserId { get; init; }
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Item
        {
            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "label")]
            public string Label { get; set; }

            [JsonProperty(PropertyName = "metadata")]
            public Metadata? Metadata { get; set; }

            public Item(string text, string label, Metadata? metadata = null)
            {
                Text = text;
                Label = label;
                Metadata = metadata;
            }
        }
#nullable restore

        public List<Item> Items { get; } = new List<Item>();

        public static async Task<ClassificationFile> ReadFromPath(string path, CancellationToken cancellationToken = default)
        {
            ClassificationFile file = new();
            foreach (string line in await File.ReadAllLinesAsync(path, cancellationToken))
            {
                if (!string.IsNullOrEmpty(line)) file.Items.Add(JsonConvert.DeserializeObject<Item>(line));
            }
            return file;
        }

        public async Task WriteToPath(string path, CancellationToken cancellationToken = default)
        {
            await File.WriteAllLinesAsync(path, Items.Select(x => JsonConvert.SerializeObject(x)), cancellationToken);
        }
    }
}
