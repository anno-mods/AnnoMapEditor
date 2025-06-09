using System;
using System.Windows;
using System.Windows.Input;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Overlays.SelectFertilities
{
    public class SelectFertilityItem : ObservableBase
    {
        public SelectFertilityItem(Action<FertilityAsset, bool> setFertility)
        {
            _setFertility = setFertility;
        }
        public FertilityAsset FertilityAsset { get; init; }

        public bool IsSelected 
        { 
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
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
        
        private Action<FertilityAsset, bool> _setFertility;

        // Added to be able to have a grid as click target and make it easier to check the checkboxes for fertility selection.
        public ICommand ToggleCommand => new ActionCommand(() => { _setFertility(FertilityAsset, IsSelected); });
    }
}
