using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Enums
{
    public class IslandDifficulty
    {
        private static readonly Logger<IslandDifficulty> _logger = new();

        public static readonly IslandDifficulty Normal = new("Normal", 0);
        public static readonly IslandDifficulty Hard   = new("Hard",   1);

        public static readonly IEnumerable<IslandDifficulty> All = new[] { Normal, Hard };


        public string Name { get; init; }

        public short ElementValue { get; init; }


        private IslandDifficulty(string name, short elementValue)
        {
            Name = name;
            ElementValue = elementValue;
        }


        public static IslandDifficulty FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Normal;

            IslandDifficulty? difficulty = All.FirstOrDefault(d => d.Name == name);

            if (difficulty is null)
            {
                _logger.LogWarning($"{name} is not a valid name for {nameof(IslandDifficulty)}. Defaulting to {nameof(Normal)}.");
                difficulty = Normal;
            }

            return difficulty;
        }

        public static IslandDifficulty FromElementValue(short? elementValue)
        {
            if (elementValue == null)
                return Normal;

            IslandDifficulty? difficulty = All.FirstOrDefault(d => d.ElementValue == elementValue);

            if (difficulty is null)
            {
                _logger.LogWarning($"{elementValue} is not a valid element value for {nameof(IslandDifficulty)}. Defaulting to {nameof(Normal)}.");
                difficulty = Normal;
            }

            return difficulty;
        }




        public override string ToString() => Name;
    }
}
