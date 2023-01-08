using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Controls
{
    public class AddIslandViewModel : DraggingViewModel
    {
        static readonly Dictionary<string, SolidColorBrush> BorderBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(255, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(255, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(255, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(255, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(255, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(255, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(255, 255, 255, 255))
        };
        static readonly Dictionary<string, SolidColorBrush> BackgroundBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(32, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(32, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(32, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(32, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(32, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(32, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(32, 255, 255, 255))
        };


        public event IslandAddedEventHandler? IslandAdded;


        public IslandType IslandType { get; init; }

        public IslandSize IslandSize { get; init; }

        public string Label { get; init; }

        public SolidColorBrush BackgroundBrush { get; init; }

        public SolidColorBrush BorderBrush { get; init; }


        public AddIslandViewModel(IslandType islandType, IslandSize islandSize)
        {
            IslandType = islandType;
            IslandSize = islandSize;
            BackgroundBrush = BackgroundBrushes[islandType.Name];
            BorderBrush = BorderBrushes[islandType.Name];

            if (islandType == IslandType.ThirdParty || islandType == IslandType.PirateIsland)
                Label = $"Random\n{islandType.Name}";
            else
                Label = $"Random\n{islandSize.Name}";
        }


        public override void OnDragged(Vector2 newPosition)
        {
            EndDrag();

            IslandAdded?.Invoke(this, new(IslandType, IslandSize, newPosition));
        }
    }
}
