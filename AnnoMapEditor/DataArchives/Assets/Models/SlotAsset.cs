using AnnoMapEditor.DataArchives.Assets.Deserialization;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AnnoMapEditor.Games;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    [AssetTemplate(TEMPLATE_NAME)]
    public class SlotAsset : StandardAsset
    {
        public const string TEMPLATE_NAME = "Slot";

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
        public SlotAsset(XElement valuesXml, GameDefaults gameDefaults)
            : base(valuesXml, gameDefaults)
        {
            DisplayName = valuesXml.Element("Text")!
                .Element("LocaText")?
                .Element("English")!
                .Element("Text")!
                .Value ?? valuesXml.Element("Standard")?
                .Element("Name")?
                .Value ?? "Unknown Slot Name";

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
