using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.Utilities;
using System;

namespace AnnoMapEditor.MapTemplates.Models
{
    public abstract class MapElement : ObservableBase
    {
        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
        private Vector2 _position = Vector2.Zero;


        public MapElement()
        {

        }


        // ---- Serialization ----

        public MapElement(Element element)
        {
            _position = new Vector2(element.Position);
        }

        public static MapElement FromTemplate(TemplateElement templateElement)
        {
            Element element = templateElement.Element!;
            MapElementType elementType = MapElementType.FromElementValue(templateElement.ElementType);

            if (elementType == MapElementType.FixedIsland)
                return new FixedIslandElement(element);

            else if (elementType == MapElementType.PoolIsland)
                return new RandomIslandElement(element);

            else if (elementType == MapElementType.StartingSpot)
                return new StartingSpotElement(element);

            else
                throw new NotImplementedException();
        }

        public TemplateElement ToTemplate()
        {
            MapElementType elementType = this switch
            {
                FixedIslandElement  _ => MapElementType.FixedIsland,
                RandomIslandElement _ => MapElementType.PoolIsland,
                StartingSpotElement _ => MapElementType.StartingSpot,
                _ => throw new NotImplementedException()
            };

            TemplateElement templateElement = new()
            {
                ElementType = elementType.ElementValue,
                Element = new()
                {
                    // The editor's coordinate system's axis are flipped compared to Anno1800. Thus we must
                    // flip X and Y when serializing.
                    Position = new int[] { Position.Y, Position.X }
                }
            };

            ToTemplate(templateElement.Element);

            return templateElement;
        }

        protected abstract void ToTemplate(Element resultElement);
    }
}
