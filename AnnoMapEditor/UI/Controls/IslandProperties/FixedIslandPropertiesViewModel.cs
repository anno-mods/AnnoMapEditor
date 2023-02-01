using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Fertilities;
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

        public FertilitiesViewModel FertilitiesViewModel { get; init; }


        public FixedIslandPropertiesViewModel(FixedIslandElement fixedIsland, Region region)
        {
            FixedIsland = fixedIsland;
            FertilitiesViewModel = new(fixedIsland, region);
            IsContinentalIsland = FixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            IslandTypeItems.AddRange(FixedIsland.IslandAsset.IslandType);
        }
    }
}
