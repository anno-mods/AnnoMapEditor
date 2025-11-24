using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Xml.Linq;
using AnnoMapEditor.Games;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class FertilityAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Fertility";


        public string DisplayName { get; init; }


        public FertilityAsset(XElement valuesXml, GameDefaults gameDefaults)
            : base(valuesXml, gameDefaults)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")?
                .Element("English")!
                .Element("Text")!
                .Value ?? valuesXml.Element("Standard")?
                .Element("Name")?
                .Value ?? "Unknown Fertility Name";
        }
    }
}
