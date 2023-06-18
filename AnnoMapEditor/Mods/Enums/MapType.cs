using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.Mods.Enums
{
    public class MapType
    {
        private static readonly Logger<MapType> _logger = new();

        public static readonly MapType Archipelago = new("Archipelago", "Archipelago", guid: "17079", assetName: "moderate_archipel");
        public static readonly MapType Atoll       = new("Atoll",       "Atoll",       guid: "17080", assetName: "moderate_atoll");
        public static readonly MapType Corners     = new("Corners",     "Corners",     guid: "17082", assetName: "moderate_corners");
        public static readonly MapType IslandArc   = new("Island Arc",  "Arc",         guid: "17081", assetName: "moderate_islandarc", templateType: "Arc");
        public static readonly MapType Snowflake   = new("Snowflake",   "Snowflake",   guid: "17083", assetName: "moderate_snowflake");

        public static readonly IEnumerable<MapType> All      = new[] { Archipelago, Atoll, Corners, IslandArc, Snowflake };


        public readonly string DisplayName;
        public readonly string Name;
        public readonly string Guid;
        public readonly string AssetName;
        public readonly char AssetNameSeparator;
        public readonly string? FileNamePart;
        public readonly string TemplateType;


        private MapType(string displayName, string name, string guid, string assetName, char assetNameSeparator = '_', string? fileNamePart = null, string? templateType = null)
        {
            DisplayName = displayName;
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


        public static MapType? FromName(string name)
        {
            MapType? type = All.FirstOrDefault(t => t.Name == name);

            if (type is null)
            {
                _logger.LogWarning($"{name} is not a valid name for {nameof(MapType)}. Defaulting to null.");
            }

            return type;
        }
    }
}
