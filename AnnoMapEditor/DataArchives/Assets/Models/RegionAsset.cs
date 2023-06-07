using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class RegionAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Region";

        public const long REGION_MODERATE_GUID = 5000000;
        public const long REGION_SOUTHAMERICA_GUID = 5000001;
        public const long REGION_ARCTIC_GUID = 160001;
        public const long REGION_AFRICA_GUID = 114327;

        [StaticAsset(REGION_MODERATE_GUID)]
        public static RegionAsset Moderate { get; private set; }

        [StaticAsset(REGION_SOUTHAMERICA_GUID)]
        public static RegionAsset SouthAmerica { get; private set; }

        [StaticAsset(REGION_ARCTIC_GUID)]
        public static RegionAsset Arctic { get; private set; }

        [StaticAsset(REGION_AFRICA_GUID)]
        public static RegionAsset Africa { get; private set; }

        public static IEnumerable<RegionAsset> SupportedRegions => new[] { Moderate, SouthAmerica, Arctic, Africa };


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

            XElement regionElement = valuesXml.Element(TEMPLATE_NAME)!;
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
