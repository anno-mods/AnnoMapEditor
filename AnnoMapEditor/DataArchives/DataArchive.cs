using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public static class DataArchive
    {
        public static readonly IDataArchive Default = new InvalidDataArchive("");


        public static async Task<IDataArchive> OpenAsync(string? folderPath)
        {
            if (folderPath is null)
                return Default;

            var adjustedPath = AdjustDataPath(folderPath);

            if (adjustedPath is null)
                return Default;

            IDataArchive archive;
            if (File.Exists(Path.Combine(adjustedPath, "maindata/data0.rda")))
            {
                RdaDataArchive rdaArchive = new RdaDataArchive(adjustedPath);
                await rdaArchive.LoadAsync();
                archive = rdaArchive;
            }
            else
                archive = new FolderDataArchive(adjustedPath);

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
}
