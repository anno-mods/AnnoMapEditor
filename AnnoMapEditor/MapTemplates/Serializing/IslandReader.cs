using AnnoMapEditor.Utilities;
using FileDBSerializing;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    internal static class IslandReader
    {
        public static int ReadTileInSizeFromFile(string mapPath)
        {
            if (Settings.Instance.DataArchive?.IsValid != true)
                return 0;

            using Stream? fs = Settings.Instance.DataArchive.OpenRead(mapPath + @"info");
            if (fs is null)
                return 0;

            var doc = ReadFileDB(fs);

            if (doc?.Roots.FirstOrDefault(x => x.Name == "MapSize" && x.NodeType == FileDBNodeType.Attrib) is not Attrib mapSize)
                return 0;

            int sizeInTiles = BitConverter.ToInt32(new ReadOnlySpan<byte>(mapSize.Content, 0, 4));
            return sizeInTiles;
        }

        public static IFileDBDocument? ReadFileDB(Stream fileStream)
        {
            try
            {
                var Version = VersionDetector.GetCompressionVersion(fileStream);
                var parser = new DocumentParser(Version);
                IFileDBDocument? doc = parser.LoadFileDBDocument(fileStream);

                return doc;
            }
            catch
            {
                // TODO error log
                return null;
            }
        }

        public static BitmapImage? ReadThumbnail(string filePath)
        {
            string thumbnailPath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                "_gamedata",
                Path.GetFileNameWithoutExtension(filePath),
                "mapimage.png");
            using Stream? stream = Settings.Instance.DataArchive?.OpenRead(thumbnailPath)!;

            BitmapImage thumbnail = new();
            thumbnail.BeginInit();
            thumbnail.StreamSource = stream;
            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            thumbnail.EndInit();
            thumbnail.Freeze();

            return thumbnail;
        }
    }
}
