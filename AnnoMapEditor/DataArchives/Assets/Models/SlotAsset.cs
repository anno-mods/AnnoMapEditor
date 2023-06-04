using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Deserialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate("Slot")]
    public class SlotAsset : StandardAsset
    {
        public const long RANDOM_MINE_OLD_WORLD_GUID = 1000029;
        public const long RANDOM_MINE_NEW_WORLD_GUID = 614;
        public const long RANDOM_MINE_ARCTIC_GUID = 116037;
        public const long RANDOM_CLAY_GUID = 100417;
        public const long RANDOM_OIL_GUID = 100849;


        public static readonly string TemplateName = "Slot";


        public string DisplayName { get; init; }

        public string? SlotType { get; init; }

        public bool IsRandomSlot { get; init; }

        public IEnumerable<long> ReplacementGuids { get; init; }

        [GuidReference(nameof(ReplacementGuids))]
        public ICollection<SlotAsset> ReplacementSlotAssets { get; set; }

        public IEnumerable<string> AssociatedRegionIds { get; init; }

        [RegionIdReference(nameof(AssociatedRegionIds))]
        public ICollection<RegionAsset> AssociatedRegions { get; set; }


        public SlotAsset() : base()
        {
            DisplayName = "";
            ReplacementGuids = Enumerable.Empty<long>();
            ReplacementSlotAssets = new SlotAsset[0];
            AssociatedRegions = new RegionAsset[0];
        }


        // deserialization constructor
        public SlotAsset(XElement valuesXml)
            : base(valuesXml)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")!
                .Element("English")!
                .Element("Text")!
                .Value!;

            SlotType = valuesXml.Element(TemplateName)?
                .Element("SlotType")?
                .Value;
            IsRandomSlot = SlotType == "Random";

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

            AssociatedRegionIds = valuesXml.Element("Building")?
                .Element("AssociatedRegions")?
                .Value?
                .Split(';')
                .ToArray()
                ?? Array.Empty<string>();
        }
    }
}
