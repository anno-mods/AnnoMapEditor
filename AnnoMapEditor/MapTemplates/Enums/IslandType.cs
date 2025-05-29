using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Enums
{
    public class IslandType
    {
        private static readonly Logger<IslandType> _logger = new();

        public static readonly IslandType Normal       = new("Normal",       null);
        public static readonly IslandType Starter      = new("Starter",      1);
        public static readonly IslandType Decoration   = new("Decoration",   2);
        public static readonly IslandType ThirdParty   = new("ThirdParty",   3);
        public static readonly IslandType PirateIsland = new("PirateIsland", 4);
        public static readonly IslandType Cliff        = new("Cliff",        5);

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


        public static IslandType FromName(string name)
        {
            IslandType? type = All.FirstOrDefault(d => d.Name == name);

            if (type is null)
            {
                _logger.LogWarning($"{name} is not a valid name for {nameof(IslandType)}. Defaulting to {nameof(Normal)}.");
                type = Normal;
            }

            return type;
        }

        public static IslandType FromElementValue(short? elementValue)
        {
            IslandType? type = All.FirstOrDefault(t => t.ElementValue == elementValue);

            if (type is null)
            {
                _logger.LogWarning($"{elementValue} is not a valid element value for {nameof(IslandType)}. Defaulting to {nameof(Normal)}/{Normal.ElementValue}.");
                type = Normal;
            }

            return type;
        }
        public static IslandType FromIslandFileName(string fileName)
        {
            switch (fileName)
            {
                case "moderate_3rdparty02_01":
                case "colony01_3rdparty05_01":
                case "moderate_3rdparty06_01":
                case "moderate_3rdparty07_01":
                case "moderate_3rdparty08_01":
                case "colony03_3rdparty09_01":
                    return ThirdParty;
                case "moderate_3rdparty03_01":
                case "colony01_3rdparty04_01":
                    return PirateIsland;
                default:
                    if (fileName.Contains("_d_")) return Decoration;
                    return Normal;
            }
        }

        public static string? DefaultIslandLabelFromFileName(string fileName)
        {
            return fileName switch
            {
                "moderate_3rdparty02_01" => "Sir Archibald Blake",
                "colony01_3rdparty05_01" => "Isabel Sarmento",
                "moderate_3rdparty06_01" => "Old Nate",
                "moderate_3rdparty07_01" => "Eli Bleakworth",
                "moderate_3rdparty08_01" => "Madame Kahina",
                "colony03_3rdparty09_01" => "Qumaq",
                "moderate_3rdparty03_01" => "Anne Harlow",
                "colony01_3rdparty04_01" => "Jean La Fortune",
                _ => null,
            };
        }


        public override string ToString() => Name;
    }
}
