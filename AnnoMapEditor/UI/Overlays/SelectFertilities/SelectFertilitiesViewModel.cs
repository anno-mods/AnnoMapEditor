using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Overlays.SelectFertilities
{
    public class SelectFertilitiesViewModel : ObservableBase, IOverlayViewModel
    {
        public FixedIslandElement FixedIsland { get; init; }

        public string? NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                UpdateFilter();
            }
        }
        private string? _nameFilter;

        public IEnumerable<RegionAsset?> Regions { get; init; } = RegionAsset.SupportedRegions;

        private readonly RegionAsset _initialRegion;

        public RegionAsset SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                _selectedRegion = value;
                UpdateFilter();

                ShowRegionWarning = _selectedRegion != _initialRegion;
            }
        }
        private RegionAsset _selectedRegion;

        public bool ShowRegionWarning
        {
            get => _showRegionWarning;
            set => SetProperty(ref _showRegionWarning, value);
        }
        private bool _showRegionWarning = false;

        public ObservableCollection<SelectFertilityItem> FertilityItems { get; init; }


        public SelectFertilitiesViewModel(RegionAsset region, FixedIslandElement fixedIsland)
        {
            // TODO: Should this happen here?
            AssetRepository assetRepository = Settings.Instance.AssetRepository!;
            FertilityItems = new(assetRepository.GetAll<RegionAsset>()
                .SelectMany(r => r.AllowedFertilities)
                .Distinct()
                .Select(f => {
                    SelectFertilityItem item = new()
                    {
                        FertilityAsset = f,
                        IsSelected = fixedIsland.Fertilities.Contains(f)
                    };
                    item.PropertyChanged += FertilityItem_PropertyChanged;
                    return item;
                })
                );

            _selectedRegion = _initialRegion = region;
            FixedIsland = fixedIsland;

            CollectionView fertilitiesView = (CollectionView)CollectionViewSource.GetDefaultView(FertilityItems);
            fertilitiesView.Filter = FertilityFilter;
        }

        private void FertilityItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SelectFertilityItem fertilityItem && e.PropertyName == nameof(SelectFertilityItem.IsSelected))
            {
                if (fertilityItem.IsSelected)
                {
                    if (!FixedIsland.Fertilities.Contains(fertilityItem.FertilityAsset))
                        FixedIsland.Fertilities.Add(fertilityItem.FertilityAsset);
                }
                else
                {
                    FixedIsland.Fertilities.Remove(fertilityItem.FertilityAsset);
                }
            }
        }

        private bool FertilityFilter(object item)
        {
            if (item is not SelectFertilityItem fertilityItem)
                return false;

            FertilityAsset fertilityAsset = fertilityItem.FertilityAsset;

            if (SelectedRegion != null && !SelectedRegion.AllowedFertilities.Contains(fertilityAsset))
                return false;
            
            else if (!string.IsNullOrEmpty(_nameFilter))
            {
                string filter = _nameFilter.ToLower();
                if (fertilityAsset.Name?.ToLower().Contains(filter) != true && !fertilityAsset.DisplayName.ToLower().Contains(filter))
                    return false;
            }

            return true;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(FertilityItems).Refresh();
        }

        public void OnClosed()
        {
            OverlayService.Instance.Close(this);
        }
    }
}
