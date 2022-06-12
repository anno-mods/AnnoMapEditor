using System;
using System.Collections.Generic;
using System.IO;

namespace AnnoMapEditor.Utils
{
    public static class DataArchive
    {
        public static IDataArchive Open(string? folderPath)
        {
            if (folderPath is null)
                return new InvalidDataPath("");

            var adjustedPath = AdjustDataPath(folderPath);

            if (adjustedPath is null)
                return new InvalidDataPath(folderPath);

            if (File.Exists(Path.Combine(adjustedPath, "maindata/data0.rda")))
                return new RdaDataArchive(adjustedPath);

            return new FolderDataArchive(adjustedPath);
        }

        private static string? AdjustDataPath(string? path)
        {
            if (path is null)
                return null;
            if (File.Exists(Path.Combine(path, "maindata/data0.rda")))
                return path;
            if (File.Exists(Path.Combine(path, "data0.rda")))
                return Path.GetDirectoryName(path);
            if (Directory.Exists(Path.Combine(path, "data/dlc01")))
                return path;
            if (Directory.Exists(Path.Combine(path, "dlc01")))
                return Path.GetDirectoryName(path);
            return null;
        }
    }

    public interface IDataArchive
    {
        bool IsValid { get; }
        string Path { get; }

        Stream? OpenRead(string filePath);
        IEnumerable<string> Find(string pattern);
    }

    public class InvalidDataPath : IDataArchive
    {
        public bool IsValid { get; } = false;
        public Stream? OpenRead(string filePath) => null;
        public string Path { get; }

        public InvalidDataPath(string path)
        {
            Path = path;
        }

        public IEnumerable<string> Find(string pattern)
        {
            return Array.Empty<string>();
        }
    }
}
