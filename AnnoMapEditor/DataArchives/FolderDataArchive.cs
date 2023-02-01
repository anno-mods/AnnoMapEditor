using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class FolderDataArchive : DataArchive
    {
        public override string DataPath { get; }

        public override bool IsValid { get; protected set; } = true;


        public FolderDataArchive(string folderPath)
        {
            DataPath = folderPath;
        }


        public override Stream? OpenRead(string filePath)
        {
            if (!IsValid)
                return null;
            Stream? stream = null;

            try
            {
                stream = File.OpenRead(System.IO.Path.Combine(DataPath, filePath));
            }
            catch { }
            return stream;
        }

        public override IEnumerable<string> Find(string pattern)
        {
            if (!IsValid)
                return Array.Empty<string>();

            Matcher matcher = new();
            matcher.AddIncludePatterns(new string[] { pattern });

            PatternMatchingResult result = matcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(DataPath)));

            return result.Files.Select(x => x.Path);
        }
    }
}
