using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        private static readonly Logger<IslandRepository> _logger = new();

        public static IslandRepository Instance = new();


        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly Dictionary<string, IslandAsset> _byFilePath = new();


        private IslandRepository()
        {
            var logger = new Logger<IslandRepository>();

            LoadAsync();
        }


        private void Add(IslandAsset island)
        {
            if (!_byFilePath.ContainsKey(island.FilePath))
            {
                _byFilePath.Add(island.FilePath, island);
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, island));
                _logger.LogInformation($"Added '{island.FilePath}'.");
            }
            else
                throw new Exception();
        }

        public bool TryGetByFilePath(string mapFilePath, [NotNullWhen(false)] out IslandAsset islandAsset)
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            => _byFilePath.TryGetValue(mapFilePath, out islandAsset);
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.

        protected override async Task DoLoad()
        {
            _logger.LogInformation($"Begin loading islands.");
            _logger.LogInformation($"Waiting for random and fixed islands to be loaded.");

            // wait for both the random and fixed island repositories to be loaded
            // TODO: BUG In theory both randomIslandRepositoryand fixedIslandRepository should be
            //       able to initialize at the same time. However, there occurs a deadlock when
            //       doing so.
            AssetRepository<RandomIslandAsset> randomIslandRepository = AssetRepository.Get<RandomIslandAsset>();
            await randomIslandRepository.AwaitLoadingAsync();

            FixedIslandRepository fixedIslandRepository = FixedIslandRepository.Instance;
            await fixedIslandRepository.AwaitLoadingAsync();

            Dictionary<string, RandomIslandAsset> randomByFilePath = randomIslandRepository
                .ToDictionary(r => r.FilePath, r => r);

            _logger.LogInformation($"Begin processing islands.");

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
                    IslandType = randomIsland?.IslandType ?? new[] { DetectIslandTypeFromPath(filePath) },
                    IslandSize = new[] { islandSize },
                    SizeInTiles = sizeInTiles,
                });
            }

            _logger.LogInformation($"Processed {fixedIslandRepository.Count()} islands.");
        }


        public static IslandType DetectIslandTypeFromPath(string filePath)
        {
            if (filePath.Contains("_d_"))
                return IslandType.Decoration;

            else
                return IslandType.Normal;
        }


        public IEnumerator<IslandAsset> GetEnumerator() => _byFilePath.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
