using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class FixedIslandRepository : Repository, INotifyCollectionChanged, IEnumerable<FixedIslandAsset>
    {
        private static readonly Logger<FixedIslandRepository> _logger = new();

        public static FixedIslandRepository Instance = new();

        private readonly List<FixedIslandAsset> _islands = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        private FixedIslandRepository()
        {
            LoadAsync();
        }


        protected override Task DoLoad()
        {
            _logger.LogInformation($"Begin loading fixed islands.");

            foreach (string mapFilePath in Settings.Instance.DataArchive.Find("**.a7m"))
            {
                FixedIslandAsset fixedIsland = new(mapFilePath);

                Add(fixedIsland);
            }

            _logger.LogInformation($"Finished loading fixed islands. Loaded {_islands.Count} islands.");

            return Task.CompletedTask;
        }


        public void Add(FixedIslandAsset fixedIsland)
        {
            _islands.Add(fixedIsland);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, fixedIsland));
            _logger.LogInformation($"Added '{fixedIsland.FilePath}'.");
        }

        public FixedIslandAsset GetByFilePath(string filePath)
        {
            return _islands.FirstOrDefault(i => i.FilePath == filePath)
                ?? throw new Exception();
        }

        public bool TryGetByFilePath(string filePath, [NotNullWhen(false)] out FixedIslandAsset? fixedIslandAsset)
        {
            fixedIslandAsset = _islands.FirstOrDefault(i => i.FilePath == filePath);
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return fixedIslandAsset != null;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }


        public IEnumerator<FixedIslandAsset> GetEnumerator() => _islands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _islands.GetEnumerator();
    }
}
