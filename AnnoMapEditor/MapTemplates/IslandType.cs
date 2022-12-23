using System;
using System.Linq;

namespace AnnoMapEditor.MapTemplates
{
    public class IslandType
    {
        public static readonly IslandType Normal       = new("Normal", 0);
        public static readonly IslandType Starter      = new("Starter", 1);
        public static readonly IslandType Decoration   = new("Decoration", 2);
        public static readonly IslandType ThirdParty   = new("ThirdParty", 3);
        public static readonly IslandType PirateIsland = new("PirateIsland", 4);
        public static readonly IslandType Cliff        = new("Cliff", 5);


        public readonly string Name;

        public readonly short ElementValue;

        public bool IsNormalOrStarter => this == Starter || this == Normal;

        public bool IsSameWithoutOil(IslandType that) => IsNormalOrStarter && that.IsNormalOrStarter;


        private IslandType(string name, short elementValue)
        {
            ElementValue = elementValue;
            Name = name;
        }

        public static IslandType FromElementValue(short elementValue)
        {
            return elementValue switch
            {
                0 => Normal,
                1 => Starter,
                2 => Decoration,
                3 => ThirdParty,
                4 => PirateIsland,
                5 => Cliff,
                _ => throw new ArgumentException($"{elementValue} is not a valid element value for IslandType.")
            };
        }


        public override string ToString() => Name;

        public override bool Equals(object? obj) => obj is IslandType other && Name.Equals(other.Name);
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator !=(IslandType a, IslandType b) => !a.Equals(b);
        public static bool operator ==(IslandType a, IslandType b) => a.Equals(b);
    }
}
