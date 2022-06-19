using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AnnoMapEditor.Mods
{
    public enum MapType
    {
        Archipelago,
        Atoll,
        Corners,
        [Description("Island Arc")]
        IslandArc,
        Snowflake
    }

    public static class MapTypes
    {
        public static IEnumerable<MapType> GetOldWorldTypes()
        {
            return new MapType[] { MapType.Archipelago, MapType.Atoll, MapType.Corners, MapType.IslandArc, MapType.Snowflake };
        }

        public static IEnumerable<MapType> GetAllTypes()
        {
            return new MapType[] { MapType.Archipelago, MapType.Atoll, MapType.Corners, MapType.IslandArc, MapType.Snowflake };
        }

        public static MapType? FromString(string that)
        {
            return that switch
            {
                nameof(MapType.Archipelago) => MapType.Archipelago,
                nameof(MapType.Atoll) => MapType.Atoll,
                nameof(MapType.Corners) => MapType.Corners,
                "Island Arc" => MapType.IslandArc,
                nameof(MapType.Snowflake) => MapType.Snowflake,
                _ => null
            };
        }
    }

    public static class MapTypeExtensions
    {
        public static bool IsOldWord(this MapType that)
        {
            return that switch
            {
                MapType.Archipelago => true,
                MapType.Atoll => true,
                MapType.Corners => true,
                MapType.IslandArc => true,
                MapType.Snowflake => true,
                _ => false
            };
        }

        public static string ToString(this MapType that)
        {
            return that switch
            {
                MapType.Archipelago => nameof(MapType.Archipelago),
                MapType.Atoll => nameof(MapType.Atoll),
                MapType.Corners => nameof(MapType.Corners),
                MapType.IslandArc => "Island Arc",
                MapType.Snowflake => nameof(MapType.Snowflake),
                _ => ""
            };
        }

        public static string? ToName(this MapType that)
        {
            return that switch
            {
                MapType.Archipelago => "moderate_archipel",
                MapType.Atoll => "moderate_atoll",
                MapType.Corners => "moderate_corners",
                MapType.IslandArc => "moderate_islandarc",
                MapType.Snowflake => "moderate_snowflake",
                _ => null
            };
        }

        public static string? ToTemplateType(this MapType that)
        {
            return that switch
            {
                MapType.Archipelago => "Archipelago",
                MapType.Atoll => "Atoll",
                MapType.Corners => "Corners",
                MapType.IslandArc => "Arc",
                MapType.Snowflake => "Snowflake",
                _ => null
            };
        }

        public static string? ToGuid(this MapType that)
        {
            return that switch
            {
                MapType.Archipelago => "17079",
                MapType.Atoll => "17080",
                MapType.Corners => "17082",
                MapType.IslandArc => "17081",
                MapType.Snowflake => "17083",
                _ => null
            };
        }
    }
}
