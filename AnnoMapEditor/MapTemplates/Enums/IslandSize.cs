using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Enums
{
    public class IslandSize
    {
        private static readonly Logger<IslandSize> _logger = new();

        public static readonly IslandSize Default     = new("Small",       null, 192);
        public static readonly IslandSize Small       = new("Small",       0,    192);
        public static readonly IslandSize Medium      = new("Medium",      1,    320);
        public static readonly IslandSize Large       = new("Large",       2,    384);
        // TODO: Placeholder for ExtraLarge. Need to figure out the details for 117.
        public static readonly IslandSize ExtraLarge  = new("ExtraLarge",  3,    400);
        public static readonly IslandSize Continental = new("Continental", 3,    int.MaxValue);

        public static readonly IEnumerable<IslandSize> All = new[] { Small, Medium, Large, Continental };


        public string Name { get; init; }

        public short? ElementValue { get; init; }

        public int DefaultSizeInTiles { get; init; }


        private IslandSize(string name, short? elementValue, int defaultSizeInTiles)
        {
            Name = name;
            ElementValue = elementValue;
            DefaultSizeInTiles = defaultSizeInTiles;
        }


        public static IslandSize FromElementValue(short? elementValue)
        {
            IslandSize? size = All.FirstOrDefault(t => t.ElementValue == elementValue);

            if (size is null)
            {
                _logger.LogWarning($"{elementValue} is not a valid element value for {nameof(IslandSize)}. Defaulting to {nameof(Default)}/{Default.ElementValue}.");
                size = Default;
            }

            return size;
        }


        public override string ToString() => Name;
    }
}
