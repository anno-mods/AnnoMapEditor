using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public class StartingSpot : MapElement
    {
        public readonly int Index;


        public StartingSpot(int index, int x, int y)
        {
            Index = index;
            Position = new Vector2(x, y);
            MapSizeInTiles = MapObject.MAP_PIN_SIZE;
        }


        public static List<MapElement> CreateStartingSpots(int playableSize, int margin)
        {
            const int SPACING = 64;

            return new()
            {
                new StartingSpot(0, margin + SPACING, playableSize + margin - SPACING),
                new StartingSpot(1, margin + SPACING, playableSize + margin - 2*SPACING),
                new StartingSpot(2, margin + 2*SPACING, playableSize + margin - SPACING),
                new StartingSpot(3, margin + 2*SPACING, playableSize + margin - 2*SPACING)
            };
        }


        public override TemplateElement ToTemplate()
        {
            TemplateElement templateElement = new();
            templateElement.ElementType = 2;
            templateElement.Element = new Element();
            templateElement.Element.Position = new int[] { Position.X, Position.Y };

            return templateElement;
        }
    }
}
