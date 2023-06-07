using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class FertilityAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Fertility";


        public string DisplayName { get; init; }


        public FertilityAsset(XElement valuesXml)
            : base(valuesXml)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")!
                .Element("English")!
                .Element("Text")!
                .Value!;
        }
    }
}
