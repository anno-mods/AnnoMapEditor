using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class PositionMarker : StandardAsset
    {
        public const string TEMPLATE_NAME = "PositionMarker";


        public PositionMarker(XElement valuesXml)
            : base(valuesXml)
        {

        }
    }
}
