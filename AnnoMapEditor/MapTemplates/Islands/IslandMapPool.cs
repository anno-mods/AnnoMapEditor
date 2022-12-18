using AnnoMapEditor.MapTemplates.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Islands
{
    public class IslandMapPool
    {
        private readonly IslandMap[] IslandMaps;

        public int Size => IslandMaps.Length;


        private IslandMapPool(params IslandMap[] islandMaps)
        {
            IslandMaps = islandMaps;
        }


        public static IslandMapPool Create(string filePathTemplate, int size)
            => Create(filePathTemplate, Enumerable.Range(0, size));

        public static IslandMapPool Create(string filePathTemplate, IEnumerable<int> indices)
        {
            IEnumerable<IslandMap> islandMaps = indices.Select(index =>
            {
                string mapFilePath = string.Format(filePathTemplate, string.Format("{0:00}", index));
                int sizeInTiles = Task.Run(() => IslandReader.ReadTileInSizeFromFileAsync(mapFilePath)).Result;
                return new IslandMap(mapFilePath, sizeInTiles);
            });

            return new IslandMapPool(islandMaps.ToArray());
        }


        public IslandMap GetRandomMap()
        {
            int index = Random.Shared.Next(IslandMaps.Length);
            return IslandMaps[index];
        }

        public IslandMap GetFromFilePath(string filePath)
        {
            return IslandMaps.FirstOrDefault(m => m.FilePath == filePath)
                ?? throw new NullReferenceException();
        }
    }
}
