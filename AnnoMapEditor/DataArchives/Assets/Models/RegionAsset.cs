using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Collections.Generic;
using System.Xml.Linq;
using AnnoMapEditor.Games;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class RegionAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Region";
        
        public string DisplayName { get; }

        public string? Ambiente { get; }

        public string RegionID { get; }
        
        public List<FertilityAsset> AllowedFertilities { get; set; } = new List<FertilityAsset>();


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

        }

        public static RegionAsset DetectFromPath(string filePath, GameDefaults gameDefaults)
        {
            return gameDefaults.GetRegionAssetFromFilePath(filePath);
        }
        
        public override string ToString() => DisplayName;
    }
}
