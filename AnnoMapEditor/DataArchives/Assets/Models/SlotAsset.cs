using AnnoMapEditor.DataArchives.Assets.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("Slot")]
    public class SlotAsset : StandardAsset
    {
        public string DisplayName { get; init; }

        public bool IsRandomSlot { get; init; }

        public IEnumerable<long> ReplacementGuids { get; init; }

        [AssetReference(nameof(ReplacementGuids))]
        public IEnumerable<SlotAsset> ReplacementSlotAssets { get; set; }


        public SlotAsset(XElement valuesXml) 
            : base(valuesXml)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")!
                .Element("English")!
                .Element("Text")!
                .Value!;

            IsRandomSlot = valuesXml.Element("Slot")?
                .Element("SlotType")?
                .Value == "Random";

            IEnumerable<long>? replacementGuids = valuesXml.Element("RandomMapObject")?
                .Element("Replacements")?
                .Elements("Item")?
                .Select(x => long.Parse(x.Element("NewAsset")!.Value))
                .ToList();

            IEnumerable<long>? dlcReplacementGuids = valuesXml.Element("RandomMapObject")?
                .Element("DLCSlotReplacements")?
                .Elements("Item")?
                .Select(x => x.Element("Replacement"))
                .Where(x => x != null)
                .Select(x => long.Parse(x!.Value))
                .ToList();

            ReplacementGuids = replacementGuids ?? dlcReplacementGuids ?? Array.Empty<long>();
        }
    }
}
