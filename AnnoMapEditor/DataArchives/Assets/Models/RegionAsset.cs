using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("Region")]
    public class RegionAsset : StandardAsset
    {
        public const long REGION_MODERATE_GUID = 5000000;
        public const long REGION_NEWWORLD_GUID = 5000001;
        public const long REGION_ARCTIC_GUID = 160001;
        public const long REGION_ENBESA_GUID = 114327;

        public const string REGION_MODERATE_REGIONID = "Moderate";


        private static readonly string TemplateName = "Region";

        /// <summary>
        /// Each region has its own AmbientName, which is needed when creating the .a7t. These
        /// values are missing in assets.xml. The values seen here were reverse engineered from
        /// existing a7t files within the game.
        /// 
        /// Note: Region assets do contain an attribute "Ambiente". However its value is always
        /// "Region_map_global" and does not match the expected value for a7ts.
        /// </summary>
        private static readonly Dictionary<long, string> REGION_AMBIENTE_HARDCODED = new Dictionary<long, string>()
        {
            [REGION_MODERATE_GUID] = "Moderate_01_day_night",
            [REGION_NEWWORLD_GUID] = "south_america_caribic_01",
            [REGION_ARCTIC_GUID] = "DLC03_01",
            [REGION_ENBESA_GUID] = "Colony_02"
        };


        public string DisplayName { get; init; }

        public string? Ambiente { get; init; }

        public string RegionID { get; init; }

        public IEnumerable<long> AllowedFertilityGuids { get; init; }

        [GuidReference(nameof(AllowedFertilityGuids))]
        public ICollection<FertilityAsset> AllowedFertilities { get; set; }
        
        
        public RegionAsset(XElement valuesXml)
            : base(valuesXml)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")?
                .Element("English")!
                .Element("Text")!
                .Value!
                ?? "Meta";

            XElement regionElement = valuesXml.Element(TemplateName)!;

            // The region Moderate does not have a RegionID specified in assets.xml. All other
            // regions have them.
            RegionID = regionElement.Element("RegionID")?.Value ?? REGION_MODERATE_REGIONID;

            AllowedFertilityGuids = regionElement.Element("AllowedFertilities")?
                .Elements("Item")?
                .Select(x => long.Parse(x.Value))
                .ToArray()
                ?? Array.Empty<long>();

            if (REGION_AMBIENTE_HARDCODED.ContainsKey(GUID))
                Ambiente = REGION_AMBIENTE_HARDCODED[GUID];
        }


        public override string ToString() => DisplayName;
    }
}
