using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Linq;

namespace AnnoMapEditor.MapTemplates
{
    public class MapElementType
    {
        public static readonly MapElementType FixedIsland  = new(null, typeof(FixedIslandElement));
        public static readonly MapElementType PoolIsland   = new(1,    typeof(RandomIslandElement));
        public static readonly MapElementType StartingSpot = new(2,    typeof(StartingSpotElement));

        public static readonly MapElementType[] All = new[] { FixedIsland, PoolIsland, StartingSpot };


        public int? ElementValue { get; init; }

        public Type ElementType { get; init; }

        
        private MapElementType(int? elementValue, Type elementType)
        {
            ElementValue = elementValue;
            ElementType = elementType;
        }


        public static MapElementType FromElementValue(int? elementValue)
        {
            MapElementType? mapElementType = All.FirstOrDefault(d => d.ElementValue == elementValue);

            if (mapElementType is null)
            {
                Log.Warn($"{elementValue} is not a valid element value for {nameof(MapElementType)}. Defaulting to {nameof(FixedIsland)}.");
                mapElementType = FixedIsland;
            }

            return mapElementType;
        }

        public static MapElementType FromMapElement(MapElement element)
        {
            return All.FirstOrDefault(d => d.ElementType == element.GetType())
                ?? throw new NotImplementedException($"Type {element.GetType()} is an unrecognized implementation of {nameof(MapElement)}.");
        }
    }
}
