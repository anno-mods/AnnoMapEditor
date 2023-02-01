using AnnoMapEditor.DataArchives.Assets.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [AssetReference(nameof(AllowedFertilityGuids))]
        public IEnumerable<FertilityAsset> AllowedFertilities { get; init; }


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
