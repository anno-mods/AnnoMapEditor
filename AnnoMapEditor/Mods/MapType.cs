using AnnoMapEditor.MapTemplates;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AnnoMapEditor.Mods
{
    public class MapType
    {
        readonly string description;
        readonly string guid;
        readonly string assetName;
        readonly char assetNameSeparator;
        readonly string? fileNamePart;
        readonly string templateType;

        //Old World
        public static readonly MapType Archipelago = new("Archipelago", guid: "17079", assetName: "moderate_archipel");
        public static readonly MapType Atoll = new("Atoll", guid: "17080", assetName: "moderate_atoll");
        public static readonly MapType Corners = new("Corners", guid: "17082", assetName: "moderate_corners");
        public static readonly MapType IslandArc = new("Island Arc", guid: "17081", assetName: "moderate_islandarc", templateType: "Arc");
        public static readonly MapType Snowflake = new("Snowflake", guid: "17083", assetName: "moderate_snowflake");

        //New World
        public static readonly MapType Colony01 = new("New World", guid: "", assetName: "SouthAmerica", ' ', fileNamePart:"colony01");

        //Lists per Session
        static readonly List<MapType> oldWorld = new() { Archipelago, Atoll, Corners, IslandArc, Snowflake };
        static readonly List<MapType> newWorld = new() { Colony01 };

        //Region/MapType Dictionary
        public static readonly Dictionary<Region, IEnumerable<MapType>> MapTypesForRegion = new Dictionary<Region, IEnumerable<MapType>>()
        {
            [Region.Moderate] = oldWorld,
            [Region.NewWorld] = newWorld
        };

        private MapType(string description, string guid, string assetName, char assetNameSeparator = '_', string? fileNamePart = null, string? templateType = null)
        {
            this.description = description;
            this.guid = guid;
            this.assetName = assetName;
            this.assetNameSeparator = assetNameSeparator;
            this.fileNamePart = fileNamePart;
            this.templateType = templateType ?? description;
        }

        public override string ToString() => description;
        public string ToGuid() => guid;
        public string ToName() => assetName + assetNameSeparator;
        public string ToFileName() => fileNamePart ?? assetName;
        public string ToTemplateType() => templateType;

        public bool IsOldWord() => oldWorld.Contains(this);
        public static IEnumerable<MapType> GetAllTypes() => new MapType[] { Archipelago, Atoll, Corners, IslandArc, Snowflake, Colony01 };

        public static MapType? FromString(string that)
        {
            return that switch
            {
                nameof(Archipelago) => Archipelago,
                nameof(Atoll) => Atoll,
                nameof(Corners) => Corners,
                "Island Arc" => IslandArc,
                nameof(Snowflake) => Snowflake,
                _ => null
            };
        }
    }
}
