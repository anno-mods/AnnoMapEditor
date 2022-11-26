using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FileDBSerializing;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    internal static class IslandReader
    {
        public static async Task<int> ReadTileInSizeFromFileAsync(string mapPath)
        {
            if (Settings.Instance.DataArchive?.IsValid != true)
                return 0;

            using Stream? fs = Settings.Instance.DataArchive.OpenRead(mapPath + @"info");
            if (fs is null)
                return 0;

            var doc = await ReadFileDBAsync(fs);

            if (doc?.Roots.FirstOrDefault(x => x.Name == "MapSize" && x.NodeType == FileDBNodeType.Attrib) is not Attrib mapSize)
                return 0;

            int sizeInTiles = BitConverter.ToInt32(new ReadOnlySpan<byte>(mapSize.Content, 0, 4));
            return sizeInTiles;
        }

        private static async Task<IFileDBDocument?> ReadFileDBAsync(Stream fileStream)
        {
            return await Task.Run(() =>
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
            });
        }
    }
}
