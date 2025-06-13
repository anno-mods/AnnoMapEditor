using System;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public class SelectFertilityItem : ObservableBase
    {
        public SelectFertilityItem(Action<FertilityAsset, bool> setFertility, FertilityAsset asset)
        {
            _setFertility = setFertility;
            FertilityAsset = asset;
        }
        
        private readonly Action<FertilityAsset, bool> _setFertility;
        
        public FertilityAsset FertilityAsset { get; init; }

        public bool IsSelected 
        { 
            get => _isSelected;
            set
            {
                _setFertility(FertilityAsset, value);
                SetProperty(ref _isSelected, value);
            }
        }
        private bool _isSelected;

        public bool IsAllowed
        {
            get => _isAllowed;
            set => SetProperty(ref _isAllowed, value);
        }
        private bool _isAllowed = true;

        public string ShortenedDisplayName => FertilityAsset.DisplayName
            .Replace(" Fertility", "")
            .Replace(" Abundance", "s");
    }
}
