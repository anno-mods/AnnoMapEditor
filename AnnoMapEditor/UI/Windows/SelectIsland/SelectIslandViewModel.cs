using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Windows.SelectIsland
{
    public class SelectIslandViewModel
    {
        public event IslandSelectedEventHandler? IslandSelected;


        public ObservableCollection<IslandAsset> Islands { get; } = new();

        public string? PathFilter 
        { 
            get => _pathFilter;
            set
            {
                _pathFilter = value;
                UpdateFilter();
            }
        }
        private string? _pathFilter;

        public IEnumerable<IslandType?> IslandTypes { get; init; } = IslandType.All;

        public IslandType? SelectedIslandType 
        { 
            get => _selectedIslandType; 
            set
            {
                _selectedIslandType = value;
                UpdateFilter();
            }
        }
        private IslandType? _selectedIslandType;

        public IEnumerable<IslandDifficulty?> IslandDifficulties { get; init; } = IslandDifficulty.All;

        public IslandDifficulty? SelectedIslandDifficulty
        {
            get => _selectedIslandDifficulty;
            set
            {
                _selectedIslandDifficulty = value;
                UpdateFilter();
            }
        }
        private IslandDifficulty? _selectedIslandDifficulty;

        public IslandAsset SelectedIsland 
        {
            get => _selectedIsland;
            set {
                _selectedIsland = value;
                IslandSelected?.Invoke(this, new(value));
            }
        }
        public IslandAsset _selectedIsland;


        public SelectIslandViewModel(IslandType? islandType = null, IslandSize? islandSize = null)
        {
            SelectedIslandType = islandType;
            // TODO: IslandSize:  SelectedIslandSize = islandSize;
            
            AssetRepository<RandomIslandAsset> randomIslandRepository = AssetRepository.Get<RandomIslandAsset>();
            randomIslandRepository.CollectionChanged += RandomIslandRepository_CollectionChanged;
            foreach (RandomIslandAsset randomIsland in randomIslandRepository)
                Islands.Add(randomIsland);

            CollectionView islandsView = (CollectionView)CollectionViewSource.GetDefaultView(Islands);
            islandsView.Filter = IslandFilter;
        }


        private bool IslandFilter(object item)
        {
            if (SelectedIslandType != null)
            {
                if (item is not RandomIslandAsset randomIsland || !randomIsland.IslandType.Contains(SelectedIslandType))
                    return false;
            }

            if (SelectedIslandDifficulty != null)
            {
                if (item is not RandomIslandAsset randomIsland || !randomIsland.IslandDifficulty.Contains(SelectedIslandDifficulty))
                    return false;
            }

            if (!string.IsNullOrEmpty(_pathFilter))
            {
                string filter = _pathFilter.ToLower();
                if (item is not IslandAsset island)
                    return false;

                if (!island.FilePath.ToLower().Contains(filter))
                    return false;
            }

            return true;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(Islands).Refresh();
        }


        private void RandomIslandRepository_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is IslandAsset island)
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Islands.Remove(island);
                        });

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is IslandAsset island)
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Islands.Add(island);
                        });
        }
    }
}
