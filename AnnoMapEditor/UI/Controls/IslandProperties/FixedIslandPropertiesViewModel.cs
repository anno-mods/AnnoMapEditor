using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.ObjectModel;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public class FixedIslandPropertiesViewModel : ObservableBase
    {
        public FixedIslandElement FixedIsland { get; init; }

        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();

        public bool IsContinentalIsland { get; init; }


        public FixedIslandPropertiesViewModel(FixedIslandElement fixedIslands)
        {
            FixedIsland = fixedIslands;
            IsContinentalIsland = FixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            IslandTypeItems.AddRange(FixedIsland.IslandAsset.IslandType);
        }
    }
}
