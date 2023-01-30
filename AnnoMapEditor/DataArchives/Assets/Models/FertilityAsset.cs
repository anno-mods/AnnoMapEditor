using AnnoMapEditor.DataArchives.Assets.Attributes;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("Fertility")]
    public class FertilityAsset : StandardAsset
    {
        public FertilityAsset(XElement valuesXml) 
            : base(valuesXml)
        {
        }
    }
}
