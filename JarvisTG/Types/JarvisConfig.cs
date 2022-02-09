using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    public class JarvisConfig
    {
        public string ConfigPath { get; }
        private readonly string openAiFileIdPath;
        private readonly string openAiFilePath;

        public JarvisConfig(string path)
        {
            ConfigPath = path;
            Directory.CreateDirectory(ConfigPath);
            openAiFileIdPath = Path.Combine(ConfigPath, "openAiFileId.txt");
            openAiFilePath = Path.Combine(ConfigPath, "openAiFile.jsonl");
        }

        public async Task<string> GetOpenAiFileId(CancellationToken cancellationToken = default)
        {
            if (File.Exists(openAiFileIdPath))
            {
                return await File.ReadAllTextAsync(openAiFileIdPath, cancellationToken);
            }
            else return null;
        }

        public async Task SetOpenAiFileId(string openAiFileId, CancellationToken cancellationToken = default)
        {
            await File.WriteAllTextAsync(openAiFileIdPath, openAiFileId, cancellationToken);
        }

        public async Task<ClassificationFile> GetClassificationFile(CancellationToken cancellationToken = default)
        {
            if (File.Exists(openAiFilePath))
            {
                return await ClassificationFile.ReadFromPath(openAiFilePath, cancellationToken);
            }
            else return null;
        }

        public async Task SaveClassificationFile(ClassificationFile classificationFile, CancellationToken cancellationToken = default)
        {
            await classificationFile.WriteToPath(openAiFilePath, cancellationToken);
        }
    }
}
