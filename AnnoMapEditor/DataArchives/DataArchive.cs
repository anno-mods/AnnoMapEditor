using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public static class DataArchive
    {
        public static readonly IDataArchive Default = new InvalidDataPath("");

        public static async Task<IDataArchive> OpenAsync(string? folderPath)
        {
            if (folderPath is null)
                return Default;

            var adjustedPath = AdjustDataPath(folderPath);

            if (adjustedPath is null)
                return Default;

            IDataArchive archive = Default;
            if (File.Exists(Path.Combine(adjustedPath, "maindata/data0.rda")))
                archive = new RdaDataArchive(adjustedPath);
            else
                archive = new FolderDataArchive(adjustedPath);

            await archive.LoadAsync();
            return archive;
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
        Task LoadAsync();
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

        public Task LoadAsync()
        {
            return Task.Run(() => { });
        }

        public IEnumerable<string> Find(string pattern)
        {
            return Array.Empty<string>();
        }
    }
}
