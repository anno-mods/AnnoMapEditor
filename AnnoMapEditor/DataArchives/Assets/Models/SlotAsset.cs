using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class SlotAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Slot";

        public const long RANDOM_MINE_OLD_WORLD_GUID = 1000029;
        public const long RANDOM_MINE_NEW_WORLD_GUID = 614;
        public const long RANDOM_MINE_ARCTIC_GUID = 116037;
        public const long RANDOM_CLAY_GUID = 100417;
        public const long RANDOM_OIL_GUID = 100849;


        [StaticAsset(RANDOM_MINE_OLD_WORLD_GUID)]
        public static SlotAsset RandomMineOldWorld { get; private set; }

        [StaticAsset(RANDOM_MINE_NEW_WORLD_GUID)]
        public static SlotAsset RandomMineNewWorld { get; private set; }

        [StaticAsset(RANDOM_MINE_ARCTIC_GUID)]
        public static SlotAsset RandomMineArctic { get; private set; }

        [StaticAsset(RANDOM_CLAY_GUID)]
        public static SlotAsset RandomClay { get; private set; }

        [StaticAsset(RANDOM_OIL_GUID)]
        public static SlotAsset RandomOil { get; private set; }


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
            ReplacementSlotAssets = Array.Empty<SlotAsset>();
            AssociatedRegions = Array.Empty<RegionAsset>();
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

            SlotType = valuesXml.Element(TEMPLATE_NAME)?
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
