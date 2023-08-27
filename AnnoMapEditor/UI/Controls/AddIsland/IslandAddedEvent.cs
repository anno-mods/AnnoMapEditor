using AnnoMapEditor.MapTemplates.Enums;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.AddIsland
{
    public delegate void IslandAddedEventHandler(object? sender, IslandAddedEventArgs e);


    public class IslandAddedEventArgs
    {
        public MapElementType MapElementType { get; init; }

        public IslandType IslandType { get; init; }
        
        public IslandSize IslandSize { get; init; }

        public Point Delta { get; init; }


        public IslandAddedEventArgs(MapElementType mapElementType, IslandType islandType, IslandSize islandSize, Point delta)
        {
            MapElementType = mapElementType;
            IslandType = islandType;
            IslandSize = islandSize;
            Delta = delta;
        }
    }
}
