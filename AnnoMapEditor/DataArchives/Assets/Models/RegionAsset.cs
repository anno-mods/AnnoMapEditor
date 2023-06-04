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
        public string DisplayName { get; init; }

        public string? Ambiente { get; init; }

        public string? RegionID { get; init; }

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

            XElement regionElement = valuesXml.Element("Region")!;
            Ambiente = regionElement.Element("Ambiente")?.Value;
            RegionID = regionElement.Element("RegionID")?.Value;

            AllowedFertilityGuids = regionElement.Element("AllowedFertilities")?
                .Elements("Item")?
                .Select(x => long.Parse(x.Value))
                .ToArray()
                ?? Array.Empty<long>();
        }
    }
}
