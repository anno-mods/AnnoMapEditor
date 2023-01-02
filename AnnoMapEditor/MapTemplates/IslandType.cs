using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.MapTemplates
{
    public class IslandType
    {
        public static readonly IslandType Normal       = new("Normal", null);
        public static readonly IslandType Starter      = new("Starter", 1);
        public static readonly IslandType Decoration   = new("Decoration", 2);
        public static readonly IslandType ThirdParty   = new("ThirdParty", 3);
        public static readonly IslandType PirateIsland = new("PirateIsland", 4);
        public static readonly IslandType Cliff        = new("Cliff", 5);

        public static readonly IEnumerable<IslandType> All = new[] { Normal, Starter, Decoration, ThirdParty, PirateIsland, Cliff };


        public readonly string Name;

        public readonly short? ElementValue;

        public bool IsNormalOrStarter => this == Starter || this == Normal;

        public bool IsSameWithoutOil(IslandType that) => ElementValue == that.ElementValue;


        private IslandType(string name, short? elementValue)
        {
            ElementValue = elementValue;
            Name = name;
        }

        public static IslandType FromElementValue(short? elementValue)
        {
            IslandType? type = All.FirstOrDefault(t => t.ElementValue == elementValue);

            if (type is null)
            {
                Log.Warn($"{elementValue} is not a valid element value for {nameof(IslandSize)}. Defaulting to {nameof(Normal)}/{Normal.ElementValue}.");
                type = Normal;
            }

            return type;
        }


        public override string ToString() => Name;
    }
}
