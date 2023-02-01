using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.UI.Controls.Fertilities
{
    public class FertilitiesViewModel
    {
        public FixedIslandElement FixedIsland { get; init; }

        public Region Region { get; init; }


        public FertilitiesViewModel(FixedIslandElement fixedIsland, Region region)
        {
            FixedIsland = fixedIsland;
            Region = region;
        }
    }
}
