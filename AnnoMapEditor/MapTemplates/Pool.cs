using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using System;
using System.Collections.Generic;
using AnnoMapEditor.DataArchives;

namespace AnnoMapEditor.MapTemplates
{
    public class Pool
    {
        private static readonly Dictionary<(RegionAsset, IslandSize), Pool> _poolsMap;
        
        static Pool()
        {
            var islandPools = DataManager.Instance.DetectedGame?.IslandPools;
            _poolsMap = new();
            
            if (islandPools is null) 
                return;
            
            foreach (Pool pool in islandPools)
                _poolsMap[(pool.Region, pool.IslandSize)] = pool;
        }
        
        public readonly RegionAsset Region;

        public readonly IslandSize IslandSize;


        public static Pool GetPool(RegionAsset region, IslandSize islandSize)
        {
            return _poolsMap[(region, islandSize)];
        }

        public static string GetRandomIslandPath(RegionAsset region, IslandSize islandSize)
        {
            // use a random Small island for IslandSize.Default
            if (islandSize == IslandSize.Default)
                islandSize = IslandSize.Small;

            Pool pool = GetPool(region, islandSize);
            int index = Random.Shared.Next(1, pool.Size);

            string path = pool.GetPath(index);
            return path;
        }


        private readonly FilePathRange[] _paths;

        public int Size
        {
            get
            {
                int sum = 0;
                foreach (var path in _paths)
                {
                    sum += path.size;
                }
                return sum;
            }
        }


        public string GetPath(int i)
        {
            int rangeIdx = 0;
            FilePathRange range = _paths[rangeIdx];
            int skipped = 0;
            while(skipped + range.size <= i)
            {
                skipped += range.size;
                range = _paths[++rangeIdx];
            }

            return range.GetPath(i - skipped);
        }


        public Pool(RegionAsset region, IslandSize islandSize, FilePathRange[] paths)
        {
            Region = region;
            IslandSize = islandSize;
            _paths = paths;
        }

        public Pool(RegionAsset region, IslandSize islandSize, string filePath, int size)
            : this(region, islandSize, new FilePathRange[]
            {
                new FilePathRange(filePath, 1, size)
            })
        {
        }

        public Pool(RegionAsset region, IslandSize islandSize, string filePath, int[] ids)
            : this(region, islandSize, new FilePathRange[]
            {
                new FilePathRange(filePath, ids)
            })
        {
        }
    }
}
