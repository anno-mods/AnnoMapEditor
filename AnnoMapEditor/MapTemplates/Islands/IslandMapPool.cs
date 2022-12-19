using AnnoMapEditor.MapTemplates.Islands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.MapTemplates
{
    public class IslandMapPool
    {
        private readonly List<IslandMap> _islandMaps;

        public int Size => _islandMaps.Count;


        public IslandMapPool(params IEnumerable<IslandMap>[] islandMaps)
        {
            _islandMaps = new List<IslandMap>();
            foreach (var maps in islandMaps)
                _islandMaps.AddRange(maps);
        }


        public IslandMap GetRandomIslandMap()
        {
            int index = Random.Shared.Next(_islandMaps.Count);
            return _islandMaps[index];
        }

        public IslandMap GetFromPath(string mapFilePath)
        {
            return _islandMaps.FirstOrDefault(m => m.FilePath == mapFilePath)
                ?? throw new NullReferenceException($"Could not find map '{mapFilePath}'.");
        }
    }
}
