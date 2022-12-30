using AnnoMapEditor.MapTemplates;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.Mods
{
    public class MapType
    {
        //Old World
        public static readonly MapType Archipelago = new("Archipelago", guid: "17079", assetName: "moderate_archipel");
        public static readonly MapType Atoll       = new("Atoll",       guid: "17080", assetName: "moderate_atoll");
        public static readonly MapType Corners     = new("Corners",     guid: "17082", assetName: "moderate_corners");
        public static readonly MapType IslandArc   = new("Island Arc",  guid: "17081", assetName: "moderate_islandarc", templateType: "Arc");
        public static readonly MapType Snowflake   = new("Snowflake",   guid: "17083", assetName: "moderate_snowflake");

        //New World
        public static readonly MapType Colony01 = new("New World", guid: "", assetName: "SouthAmerica", ' ', fileNamePart: "colony01");

        //Lists
        public static readonly IEnumerable<MapType> OldWorld = new[] { Archipelago, Atoll, Corners, IslandArc, Snowflake };
        public static readonly IEnumerable<MapType> NewWorld = new[] { Colony01 };
        public static readonly IEnumerable<MapType> All      = new[] { Archipelago, Atoll, Corners, IslandArc, Snowflake, Colony01 };

        //Region/MapType Dictionary
        public static readonly Dictionary<Region, IEnumerable<MapType>> MapTypesForRegion = new()
        {
            [Region.Moderate] = OldWorld,
            [Region.NewWorld] = NewWorld
        };


        public readonly string Name;
        public readonly string Guid;
        public readonly string AssetName;
        public readonly char AssetNameSeparator;
        public readonly string? FileNamePart;
        public readonly string TemplateType;


        private MapType(string name, string guid, string assetName, char assetNameSeparator = '_', string? fileNamePart = null, string? templateType = null)
        {
            Name = name;
            Guid = guid;
            AssetName = assetName;
            AssetNameSeparator = assetNameSeparator;
            FileNamePart = fileNamePart;
            TemplateType = templateType ?? name;
        }

        public override string ToString() => Name;
        public string ToName() => AssetName + AssetNameSeparator;
        public string ToFileName() => FileNamePart ?? AssetName;

        public static MapType? FromString(string name)
        {
            MapType? type = All.FirstOrDefault(t => t.Name == name);

            if (type is null)
            {
                Log.Warn($"{name} is not a valid name value for {nameof(MapType)}. Defaulting to null.");
            }

            return type;
        }
    }
}
