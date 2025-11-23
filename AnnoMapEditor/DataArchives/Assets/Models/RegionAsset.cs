using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AnnoMapEditor.Games;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class RegionAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Region";
        
        public string DisplayName { get; init; }

        public string? Ambiente { get; init; }

        public string RegionID { get; set; }

        public IEnumerable<long> AllowedFertilityGuids { get; init; }

        [GuidReference(nameof(AllowedFertilityGuids))]
        public ICollection<FertilityAsset> AllowedFertilities { get; init; }


        public RegionAsset(XElement valuesXml, GameDefaults gameDefaults)
            : base(valuesXml, gameDefaults)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")?
                .Element("English")!
                .Element("Text")!
                .Value ?? valuesXml.Element("Standard")?
                .Element("Name")?
                .Value ?? "Unknown Region Name";

            XElement regionElement = valuesXml.Element(TEMPLATE_NAME)!;

            if (gameDefaults.RegionAmbienteDictionary.TryGetValue(GUID, out var regionAmbiente))
                Ambiente = regionAmbiente;

            // The default region does not have a RegionID specified in assets.xml. All other
            // regions have them.
            RegionID = regionElement.Element("RegionID")?.Value ?? gameDefaults.DefaultRegionId;

            AllowedFertilityGuids = regionElement.Element("AllowedFertilities")?
                .Elements("Item")?
                .Select(x => long.Parse(x.Value))
                .ToArray()
                ?? Array.Empty<long>();
        }

        public static RegionAsset DetectFromPath(string filePath, GameDefaults gameDefaults)
        {
            return gameDefaults.GetRegionAssetFromFilePath(filePath);
        }
        
        public override string ToString() => DisplayName;
    }
}
