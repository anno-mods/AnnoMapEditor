using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.AddIsland
{
    public delegate void IslandAddedEventHandler(object? sender, IslandAddedEventArgs e);


    public class IslandAddedEventArgs
    {
        public MapElementType MapElementType { get; init; }

        public IslandType IslandType { get; init; }
        
        public IslandSize IslandSize { get; init; }

        public Vector2 Delta { get; init; }


        public IslandAddedEventArgs(MapElementType mapElementType, IslandType islandType, IslandSize islandSize, Vector2 delta)
        {
            MapElementType = mapElementType;
            IslandType = islandType;
            IslandSize = islandSize;
            Delta = delta;
        }
    }
}
