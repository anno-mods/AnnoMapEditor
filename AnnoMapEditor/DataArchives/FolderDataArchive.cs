using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class FolderDataArchive : IDataArchive
    {
        public string Path { get; }

        public bool IsValid { get; } = true;


        public FolderDataArchive(string folderPath)
        {
            Path = folderPath;
        }


        public Task LoadAsync()
        {
            return Task.Run(() => { });
        }

        public Stream? OpenRead(string filePath)
        {
            if (!IsValid)
                return null;
            Stream? stream = null;

            try
            {
                stream = File.OpenRead(System.IO.Path.Combine(Path, filePath));
            }
            catch { }
            return stream;
        }

        public IEnumerable<string> Find(string pattern)
        {
            if (!IsValid)
                return Array.Empty<string>();

            Matcher matcher = new();
            matcher.AddIncludePatterns(new string[] { pattern });

            PatternMatchingResult result = matcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(Path)));

            return result.Files.Select(x => x.Path);
        }
    }
}
