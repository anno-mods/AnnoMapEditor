using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Overlays.SelectIsland
{
    public class SelectIslandViewModel : ObservableBase, IOverlayViewModel
    {
        public event IslandSelectedEventHandler? IslandSelected;


        public IslandRepository Islands { get; } = Settings.Instance.IslandRepository!;

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

        public IEnumerable<RegionAsset?> Regions { get; init; } = RegionAsset.SupportedRegions;

        private readonly RegionAsset _initialRegion;

        public RegionAsset? SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                _selectedRegion = value;
                UpdateFilter();

                ShowRegionWarning = _selectedRegion != _initialRegion;
            }
        }
        private RegionAsset? _selectedRegion;

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

        public IEnumerable<IslandSize?> IslandSizes { get; init; } = IslandSize.All;

        public IslandSize? SelectedIslandSize
        {
            get => _selectedIslandSize;
            set
            {
                _selectedIslandSize = value;
                UpdateFilter();
            }
        }
        private IslandSize? _selectedIslandSize;

        public IslandAsset? SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                if (value != null)
                {
                    _selectedIsland = value;
                    OverlayService.Instance.Close(this);
                    IslandSelected?.Invoke(this, new(value));
                }
            }
        }
        public IslandAsset? _selectedIsland;

        public bool ShowRegionWarning
        {
            get => _showRegionWarning;
            set => SetProperty(ref _showRegionWarning, value);
        }
        private bool _showRegionWarning = false;


        public SelectIslandViewModel(RegionAsset region, IslandType? islandType = null, IslandSize? islandSize = null)
        {
            SelectedRegion = _initialRegion = region;
            SelectedIslandType = islandType;
            SelectedIslandSize = islandSize;
            
            CollectionView islandsView = (CollectionView)CollectionViewSource.GetDefaultView(Islands);
            islandsView.Filter = IslandFilter;
        }


        private bool IslandFilter(object item)
        {
            if (SelectedRegion != null)
            {
                if (item is not IslandAsset island || island.Region != SelectedRegion)
                    return false;
            }

            if (SelectedIslandType != null)
            {
                if (item is not IslandAsset island || !island.IslandType.Contains(SelectedIslandType))
                    return false;
            }

            if (SelectedIslandDifficulty != null)
            {
                if (item is not IslandAsset island || !island.IslandDifficulty.Contains(SelectedIslandDifficulty))
                    return false;
            }

            if (SelectedIslandSize != null)
            {
                if (item is not IslandAsset island || !island.IslandSize.Contains(SelectedIslandSize))
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

        public void OnCancel()
        {
            OverlayService.Instance.Close(this);
        }

    }
}
