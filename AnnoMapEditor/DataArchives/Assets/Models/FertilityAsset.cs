using AnnoMapEditor.DataArchives.Assets.Attributes;
using System.Windows.Media;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("Fertility")]
    public class FertilityAsset : StandardAsset
    {
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
