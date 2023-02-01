using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Overlays.SelectFertilities
{
    public class SelectFertilityItem : ObservableBase
    {
        public FertilityAsset FertilityAsset { get; init; }

        public bool IsSelected 
        { 
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private bool _isSelected;
    }
}
