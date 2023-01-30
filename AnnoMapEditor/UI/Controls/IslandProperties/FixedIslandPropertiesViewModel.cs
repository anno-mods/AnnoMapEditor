using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.ObjectModel;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public class FixedIslandPropertiesViewModel : ObservableBase
    {
        public FixedIslandElement FixedIsland { get; init; }

        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();

        public bool RandomRotation
        {
            get => FixedIsland.Rotation == null;
            set
            {
                FixedIsland.Rotation = value ? null : 0;
                OnPropertyChanged();
            }
        }


        public FixedIslandPropertiesViewModel(FixedIslandElement fixedIslands)
        {
            FixedIsland = fixedIslands;

            IslandTypeItems.AddRange(FixedIsland.IslandAsset.IslandType);
        }
    }
}
