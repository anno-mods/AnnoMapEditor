using AnnoMapEditor.DataArchives.Assets.Models;

namespace AnnoMapEditor.UI.Overlays.SelectIsland
{
    public delegate void IslandSelectedEventHandler(object? sender, IslandSelectedEventArgs e);


    public class IslandSelectedEventArgs
    {
        public IslandAsset IslandAsset { get; init; }


        public IslandSelectedEventArgs(IslandAsset islandAsset)
        {
            IslandAsset = islandAsset;
        }
    }
}
