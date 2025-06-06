using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Fertilities;
using AnnoMapEditor.UI.Controls.Slots;
using AnnoMapEditor.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using AnnoMapEditor.Utilities.UndoRedo;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public class FixedIslandPropertiesViewModel : ObservableBase
    {
        public FixedIslandElement FixedIsland { get; init; }

        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();

        public bool ShowIslandTypeItems { get; init; }

        public bool IsContinentalIsland { get; init; }

        public FertilitiesViewModel FertilitiesViewModel { get; init; }

        public SlotsViewModel SlotsViewModel { get; init; }


        public FixedIslandPropertiesViewModel(FixedIslandElement fixedIsland, RegionAsset region)
        {
            FixedIsland = fixedIsland;
            FertilitiesViewModel = new(fixedIsland, region);
            SlotsViewModel = new(fixedIsland, region);
            IsContinentalIsland = FixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            IslandTypeItems.AddRange(FixedIsland.IslandAsset.IslandType);
            ShowIslandTypeItems = IslandTypeItems.Count > 1;
        }

        public void RotateIsland(bool clockwise)
        {
            byte islandRotation = FixedIsland.Rotation ?? 0;
            if (clockwise)
                FixedIsland.Rotation = (byte)((islandRotation - 1) % 4);
            else 
                FixedIsland.Rotation = (byte)((islandRotation + 1) % 4);
            
            UndoRedoStack.Instance.Do(
                new MapElementTransformStackEntry(
                    FixedIsland, 
                    FixedIsland.Position, FixedIsland.Position,
                    islandRotation, FixedIsland.Rotation
                )
            );
        }

        public void SetIslandRotationRandom()
        {
            FixedIsland.Rotation = null;
        }
    }
}
