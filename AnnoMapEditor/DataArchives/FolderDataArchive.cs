using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnnoMapEditor.DataArchives
{
    public class FolderDataArchive : DataArchive
    {
        public string DataPath { get; }


        public FolderDataArchive(string dataPath)
        {
            DataPath = dataPath;
        }


        public override Stream? OpenRead(string filePath)
        {
            try
            {
                return File.OpenRead(Path.Combine(DataPath, filePath));
            }
            catch
            {
                return null;
            }
        }

        public override IEnumerable<string> Find(string pattern)
        {
            Matcher matcher = new();
            matcher.AddIncludePatterns(new string[] { pattern });

            PatternMatchingResult result = matcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(DataPath))
                );

            return result.Files.Select(x => x.Path);
        }
    }
}
