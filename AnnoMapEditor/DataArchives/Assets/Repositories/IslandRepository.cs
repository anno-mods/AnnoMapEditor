using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Serializing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    /// <summary>
    /// All Islands exist within the game's RDA archives as an `.a7m` file. Additionally a subset
    /// of islands is defined as `RandomIsland` assets in `assets.xml`.
    /// 
    /// `RandomIsland`s are assigned to pools based on their IslandRegion, IslandType, IslandSize
    /// and IslandDifficulty. When the map generator encounters a `MapElementType.PoolIsland`, it
    /// searches for a random suitable `RandomIsland` and places it on the map.
    /// 
    /// Islands which don't appear as `RandomIsland` in assets.xml will never show up at random.
    /// However it is possible to place them on a map by using their filepath from the RDAs 
    /// directory.
    /// 
    /// This repository creates a full list of all Islands based on the results from 
    /// `FixedIslandRepository` and `AssetRepository<RandomIsland>`.
    /// </summary>
    public class IslandRepository : Repository, IEnumerable<IslandAsset>, INotifyCollectionChanged
    {
        public static IslandRepository Instance = new();


        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly Dictionary<string, IslandAsset> _byFilePath = new();


        private IslandRepository()
        {
            LoadAsync();
        }


        private void Add(IslandAsset island)
        {
            if (!_byFilePath.ContainsKey(island.FilePath))
            {
                _byFilePath.Add(island.FilePath, island);
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, island));
            }
            else
                throw new Exception();
        }

        protected override async Task DoLoad()
        {
            // wait for both the random and fixed island repositories to be loaded
            AssetRepository<RandomIslandAsset> randomIslandRepository = AssetRepository.Get<RandomIslandAsset>();
            FixedIslandRepository fixedIslandRepository = FixedIslandRepository.Instance;

            await randomIslandRepository.AwaitLoadingAsync();
            await fixedIslandRepository.AwaitLoadingAsync();

            Dictionary<string, RandomIslandAsset> randomByFilePath = randomIslandRepository
                .ToDictionary(r => r.FilePath, r => r);

            // merge random and fixed islands
            foreach (FixedIslandAsset fixedIsland in fixedIslandRepository)
            {
                string filePath = fixedIsland.FilePath;
                randomByFilePath.TryGetValue(filePath, out RandomIslandAsset? randomIsland);

                BitmapImage thumbnail = IslandReader.ReadThumbnail(filePath)!;
         
                int sizeInTiles = IslandReader.ReadTileInSizeFromFile(filePath)!;
                IslandSize islandSize = IslandSize.All.FirstOrDefault(s => sizeInTiles <= s.DefaultSizeInTiles)!;
                
                Add(new()
                {
                    FilePath = filePath,
                    DisplayName = randomIsland?.Name ?? Path.GetFileNameWithoutExtension(filePath),
                    Thumbnail = thumbnail,
                    Region = randomIsland?.IslandRegion ?? Region.DetectFromPath(filePath),
                    IslandDifficulty = randomIsland?.IslandDifficulty ?? new[] { IslandDifficulty.Normal },
                    IslandType = randomIsland?.IslandType ?? new[] { IslandType.Normal },
                    IslandSize = new[] { islandSize },
                    SizeInTiles = sizeInTiles,
                });
            }
        }


        public IEnumerator<IslandAsset> GetEnumerator() => _byFilePath.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
