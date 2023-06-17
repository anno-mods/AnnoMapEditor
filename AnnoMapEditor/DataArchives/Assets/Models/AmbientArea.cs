using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class AmbientArea : StandardAsset
    {
        public const string TEMPLATE_NAME = "AmbientArea";


        public AmbientArea(XElement valuesXml)
            : base(valuesXml)
        {

        }
    }
}
