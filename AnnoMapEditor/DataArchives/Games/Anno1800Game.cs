using System;
using System.Collections.Generic;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;

namespace AnnoMapEditor.DataArchives.Games
{
    internal class Anno1800Game : Game
    {
        public override string Title => "Anno 1800";
        public override string IconGeometry => "M4,18V20H8V18H4M4,14V16H14V14H4M10,18V20H14V18H10M16,14V16H20V14H16M16,18V20H20V18H16M2,22V8L7,12V8L12,12V8L17,12L18,2H21L22,12V22H2Z";
        public override string AssetsXmlPath => "data/config/export/main/asset/assets.xml";
        public override StaticGameAssets StaticAssets => new Anno1800StaticAssets();
    }
    
    internal class Anno1800StaticAssets : StaticGameAssets
    {
        // Region GUIDs
        public const long RegionModerateGuid = 5000000;
        public const long RegionSouthAmericaGuid = 5000001;
        public const long RegionArcticGuid = 160001;
        public const long RegionAfricaGuid = 114327;
        
        // Session GUIDs
        public const long SessionOldWorldGuid = 180023;
        public const long SessionNewWorldGuid = 180025;
        public const long SessionSunkenTreasuresGuid = 110934;
        public const long SessionArcticGuid = 180045;
        public const long SessionEnbesaGuid = 112132;
        
        // Slot GUIDs
        public const long RandomMineOldWorldGuid = 1000029;
        public const long RandomMineNewWorldGuid = 614;
        public const long RandomMineArcticGuid = 116037;
        public const long RandomClayGuid = 100417;
        public const long RandomOilGuid = 100849;
        
        // Static Region Assets
        [StaticAsset(RegionModerateGuid)]
        public static RegionAsset? ModerateRegion { get; private set; }

        [StaticAsset(RegionSouthAmericaGuid)]
        public static RegionAsset? SouthAmericaRegion { get; private set; }

        [StaticAsset(RegionArcticGuid)]
        public static RegionAsset? ArcticRegion { get; private set; }

        [StaticAsset(RegionAfricaGuid)]
        public static RegionAsset? AfricaRegion { get; private set; }
        
        // Static Session Assets
        [StaticAsset(SessionOldWorldGuid)]
        public static SessionAsset? OldWorldSession { get; private set; }

        [StaticAsset(SessionNewWorldGuid)]
        public static SessionAsset? NewWorldSession { get; private set; }

        [StaticAsset(SessionSunkenTreasuresGuid)]
        public static SessionAsset? CapeTrelawneySession { get; private set; }

        [StaticAsset(SessionArcticGuid)]
        public static SessionAsset? ArcticSession { get; private set; }

        [StaticAsset(SessionEnbesaGuid)]
        public static SessionAsset? EnbesaSession { get; private set; }
        
        // Static slot assets
        [StaticAsset(RandomMineOldWorldGuid)]
        public static SlotAsset? RandomMineOldWorld { get; private set; }

        [StaticAsset(RandomMineNewWorldGuid)]
        public static SlotAsset? RandomMineNewWorld { get; private set; }

        [StaticAsset(RandomMineArcticGuid)]
        public static SlotAsset? RandomMineArctic { get; private set; }

        [StaticAsset(RandomClayGuid)]
        public static SlotAsset? RandomClay { get; private set; }

        [StaticAsset(RandomOilGuid)]
        public static SlotAsset? RandomOil { get; private set; }


        public override IEnumerable<RegionAsset?> SupportedRegions => new[] { ModerateRegion, SouthAmericaRegion, ArcticRegion, AfricaRegion };
        public override IEnumerable<SessionAsset?> SupportedSessions => new [] { OldWorldSession, NewWorldSession, CapeTrelawneySession, ArcticSession, EnbesaSession };
        public override IEnumerable<SlotAsset?> SupportedSlots => new [] { RandomMineOldWorld, RandomMineNewWorld, RandomMineArctic, RandomClay, RandomOil };

        public override IEnumerable<Type> SupportedAssetTypes => new[]
        {
            typeof(RegionAsset),
            typeof(FertilityAsset),
            typeof(RandomIslandAsset),
            typeof(SlotAsset),
            // TODO: What is this Asset Type used for?
            // typeof(MinimapSceneAsset),
            typeof(SessionAsset),
            typeof(MapTemplateAsset)
        };
    }
}