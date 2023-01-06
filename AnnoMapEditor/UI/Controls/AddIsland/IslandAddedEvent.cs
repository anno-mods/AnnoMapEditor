using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls
{
    public delegate void IslandAddedEventHandler(object? sender, IslandAddedEventArgs e);


    public class IslandAddedEventArgs
    {
        public IslandType IslandType { get; init; }

        public IslandSize IslandSize { get; init; }

        public Vector2 Position { get; init; }


        public IslandAddedEventArgs(IslandType islandType, IslandSize islandSize, Vector2 position)
        {
            IslandType = islandType;
            IslandSize = islandSize;
            Position   = position;
        }
    }
}
