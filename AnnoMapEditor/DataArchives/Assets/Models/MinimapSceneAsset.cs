using AnnoMapEditor.DataArchives.Assets.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("MinimapScene")]
    public class MinimapSceneAsset : StandardAsset
    {
        public const long GUID = 500204;


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
