using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FileDBSerializing;
using AnnoMapEditor.Utilities;

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

        private static IFileDBDocument? ReadFileDB(Stream fileStream)
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
    }
}
