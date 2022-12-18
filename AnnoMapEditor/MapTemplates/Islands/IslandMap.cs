using AnnoMapEditor.MapTemplates.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Islands
{
    public class IslandMap
    {
        public readonly string FilePath;

        public readonly int SizeInTiles;


        public IslandMap(string filePath, int sizeInTiles)
        {
            FilePath = filePath;
            SizeInTiles = sizeInTiles;
        }


        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, int count)
            => FromFilePathTemplate(filePathTemplate, Enumerable.Range(0, count).ToArray());

        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, int start, int count)
            => FromFilePathTemplate(filePathTemplate, Enumerable.Range(start, count).ToArray());

        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, IEnumerable<int> indices)
        {
            return indices.Select(index =>
            {
                string mapFilePath = string.Format(filePathTemplate, string.Format("{0:00}", index));
                int sizeInTiles = Task.Run(() => IslandReader.ReadTileInSizeFromFileAsync(mapFilePath)).Result;
                return new IslandMap(mapFilePath, sizeInTiles);
            });
        }
    }
}
