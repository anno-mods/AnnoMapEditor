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
        readonly string templateType;

        public static readonly MapType Archipelago = new("Archipelago", guid: "17079", assetName: "moderate_archipel");
        public static readonly MapType Atoll = new("Atoll", guid: "17080", assetName: "moderate_atoll");
        public static readonly MapType Corners = new("Corners", guid: "17082", assetName: "moderate_corners");
        public static readonly MapType IslandArc = new("Island Arc", guid: "17081", assetName: "moderate_islandarc", templateType: "Arc");
        public static readonly MapType Snowflake = new("Snowflake", guid: "17083", assetName: "moderate_snowflake");

        private MapType(string description, string guid, string assetName, string? templateType = null)
        {
            this.description = description;
            this.guid = guid;
            this.assetName = assetName;
            this.templateType = templateType ?? description;
        }

        public override string ToString() => description;
        public string ToGuid() => guid;
        public string ToName() => assetName;
        public string ToTemplateType() => templateType;

        static readonly List<MapType> oldWorld = new() { Archipelago, Atoll, Corners, IslandArc, Snowflake };
        public bool IsOldWord() => oldWorld.Contains(this);
        public static IEnumerable<MapType> GetOldWorldTypes() => oldWorld;
        public static IEnumerable<MapType> GetAllTypes() => new MapType[] { Archipelago, Atoll, Corners, IslandArc, Snowflake };

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
