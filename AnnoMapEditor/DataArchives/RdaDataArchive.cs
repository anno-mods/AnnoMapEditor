using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using RDAExplorer;

namespace AnnoMapEditor.DataArchives
{
    public static class RDAExtensions
    {
        public static RDAFile? GetFileByPath(this RDAReader that, string path)
        {
            Queue<string>? parts = new(Path.GetDirectoryName(path)?.Split('\\') ?? Array.Empty<string>());
            if (!parts.Any())
                return null;

            RDAFolder? currentFolder = that.rdaFolder;

            while (parts.Count > 0 && currentFolder is not null)
            {
                var part = parts.Dequeue();
                currentFolder = currentFolder.Folders.FirstOrDefault(x => x.Name == part);
            }

            if (currentFolder is null)
                return null;

            var fileName = Path.GetFileName(path);
            RDAFile? file = currentFolder.Files.FirstOrDefault(x => Path.GetFileName(x.FileName) == fileName);
            return file;
        }
    }

    public class RdaDataArchive : IDataArchive, IDisposable
    {
        public string Path { get; }
        public bool IsValid { get; } = true;

        readonly RDAReader[]? readers;

        readonly Dictionary<string, RDAFile> allFiles = new();

        public RdaDataArchive(string folderPath)
        {
            Path = folderPath;

            // let's skip a few to speed up the loading: 0, 1, 2, 3, 4, 7, 8, 9
            var archives = Directory.
                GetFiles(System.IO.Path.Combine(Path, "maindata"), "*.rda")
                // filter some rda we don't use for sure
                .Where(x => !x.EndsWith("data0.rda") && !x.EndsWith("data1.rda") && !x.EndsWith("data2.rda") && !x.EndsWith("data3.rda")
                    && !x.EndsWith("data4.rda") && !x.EndsWith("data7.rda") && !x.EndsWith("data8.rda") && !x.EndsWith("data9.rda"))
                // load highest numbers first
                .Reverse();
            readers = archives.Select(x =>
            {
                try
                {
                    var reader = new RDAReader
                    {
                        FileName = x
                    };
                    reader.ReadRDAFile();
                    foreach (var file in reader.rdaFolder.GetAllFiles())
                        if (file.FileName.EndsWith(".a7tinfo") || file.FileName.EndsWith(".png") || file.FileName.EndsWith(".a7minfo"))
                            allFiles[file.FileName] = file;
                    return reader;
                }
                catch
                {
                    return null;
                }
            }).Where(x => x is not null).Select(x => x!).ToArray();
        }

        public Stream? OpenRead(string filePath)
        {
            if (!IsValid || readers is null)
                return null;
            Stream? stream = null;

            try
            {
                // RDAFile? file = readers.Select(x => x.GetFileByPath(filePath)).Where(x => x is not null)!.FirstOrDefault();
                if (!allFiles.TryGetValue(filePath.Replace('\\', '/'), out RDAFile? file) || file is null)
                    return null;

                stream = new MemoryStream(file.GetData());
            }
            catch { }
            return stream;
        }

        public IEnumerable<string> Find(string pattern)
        {
            if (!IsValid || readers is null)
                return Array.Empty<string>();

            Matcher matcher = new();
            matcher.AddIncludePatterns(new string[] { pattern });

            PatternMatchingResult result = matcher.Match(allFiles.Keys);

            return result.Files.Select(x => x.Path);
        }

        public void Dispose()
        {
            if (readers is not null)
            {
                foreach (var reader in readers)
                    reader.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
