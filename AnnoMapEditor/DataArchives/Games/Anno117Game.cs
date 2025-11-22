using System;
using System.Collections.Generic;
using System.Linq;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;

namespace AnnoMapEditor.DataArchives.Games
{
    internal class Anno117Game : Game
    {
        public override string Title => "Anno 117 - Pax Romana";
        public override string IconGeometry => "M6,5H18A1,1 0 0,1 19,6A1,1 0 0,1 18,7H6A1,1 0 0,1 5,6A1,1 0 0,1 6,5M21,2V4H3V2H21M15,8H17V22H15V8M7,8H9V22H7V8M11,8H13V22H11V8Z";
        public override string AssetsXmlPath => "data/base/config/export/assets.xml";
        public override StaticGameAssets StaticAssets => new Anno117StaticAssets();
    }
    
    internal class Anno117StaticAssets : StaticGameAssets
    {
        // Region GUIDs
        public const long RegionRomanGuid = 3225;
        public const long RegionCelticGuid = 6626;
        
        // Session GUIDs
        public const long SessionLatiumGuid = 3245;
        public const long SessionAlbionGuid = 6627;
        
        // Region Assets
        [StaticAsset(RegionRomanGuid)]
        public static RegionAsset? Roman { get; private set; }

        [StaticAsset(RegionCelticGuid)]
        public static RegionAsset? Celtic { get; private set; }
        
        // Session Assets
        [StaticAsset(SessionLatiumGuid)]
        public static SessionAsset? Latium { get; private set; }
        
        [StaticAsset(SessionAlbionGuid)]
        public static SessionAsset? Albion { get; private set; }
        
        public override IEnumerable<RegionAsset?> SupportedRegions => new [] { Roman, Celtic };
        public override IEnumerable<SessionAsset?> SupportedSessions => new[] { Latium, Albion };
        public override IEnumerable<SlotAsset?> SupportedSlots => Enumerable.Empty<SlotAsset>(); // Anno 117 does not use slots.
        public override IEnumerable<Type> SupportedAssetTypes => new[]
        {
            typeof(RegionAsset),
            typeof(FertilityAsset),
            typeof(RandomIslandAsset),
            // typeof(MinimapSceneAsset),
            typeof(SessionAsset),
            typeof(MapTemplateAsset)
        };
    }
}