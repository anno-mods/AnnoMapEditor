using System;

namespace AnnoMapEditor.MapTemplates
{
    public class IslandSize
    {
        public static readonly IslandSize Default = new("Default", null, 192);
        public static readonly IslandSize Small   = new("Small",   0,    192);
        public static readonly IslandSize Medium  = new("Medium",  1,    320);
        public static readonly IslandSize Large   = new("Large",   2,    384);


        public readonly string Name;

        public readonly short? ElementValue;

        public readonly int DefaultSizeInTiles;


        private IslandSize(string name, short? elementValue, int defaultSizeInTiles)
        {
            Name = name;
            ElementValue = elementValue;
            DefaultSizeInTiles = defaultSizeInTiles;
        }

        public static IslandSize FromElementValue(short? elementValue)
        {
            return elementValue switch
            {
                null => Default,
                0 => Small,
                1 => Medium,
                2 => Large,
                _ => throw new ArgumentException($"{elementValue} is not a valid element value for IslandSize.")
            };
        }


        public override string ToString() => Name;

        public override bool Equals(object? obj) => obj is IslandSize other && Name.Equals(other.Name);
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator !=(IslandSize a, IslandSize b) => !a.Equals(b);
        public static bool operator ==(IslandSize a, IslandSize b) => a.Equals(b);
    }
}
