using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class VisualObject : StandardAsset
    {
        public const string TEMPLATE_NAME = "VisualObject";


        public VisualObject(XElement valuesXml)
            : base(valuesXml)
        {

        }
    }
}
