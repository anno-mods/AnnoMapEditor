using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class MinimapSceneAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "MinimapScene";

        public const long INSTANCE_GUID = 500204;


        [StaticAsset(INSTANCE_GUID)]
        public static MinimapSceneAsset Instance { get; set; }


        public List<long> FertilityOrderGuids { get; init; }

        public List<string> LodesOrderSlotTypes { get; init; }


        // deserialization constructor
        public MinimapSceneAsset(XElement valuesXml)
            : base(valuesXml)
        {
            FertilityOrderGuids = valuesXml.Element("MinimapScene")?
                .Element("FertilityOrder")?
                .Elements("Item")
                .Select(x => long.Parse(x.Element("GUID")!.Value))
                .ToList()!;

            LodesOrderSlotTypes = valuesXml.Element("MinimapScene")?
                .Element("LodesOrder")?
                .Elements("Item")?
                .Select(x => x.Element("SlotType")!.Value)
                .Where(x => x != null)
                .ToList()!;
        }
    }
}
