using System;
using System.Collections.Generic;
using System.Linq;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;

namespace AnnoMapEditor.Games
{
    internal class Anno117Game : Game
    {
        public override string Title => "Anno 117 - Pax Romana";
        public override string IconGeometry => "M6,5H18A1,1 0 0,1 19,6A1,1 0 0,1 18,7H6A1,1 0 0,1 5,6A1,1 0 0,1 6,5M21,2V4H3V2H21M15,8H17V22H15V8M7,8H9V22H7V8M11,8H13V22H11V8Z";
        public override string AssetsXmlPath => "data/base/config/export/assets.xml";
        public override StaticGameAssets StaticAssets => new Anno117StaticAssets();

        public override IEnumerable<Pool> IslandPools => new List<Pool>()
        {
            // Roman
            new(Anno117StaticAssets.RomanRegion, IslandSize.Small, "data/base/provinces/roman/islands/pool/roman_island_small_{0}/roman_island_small_{0}.a7m", 7),
            new(Anno117StaticAssets.RomanRegion, IslandSize.Medium, "data/base/provinces/roman/islands/pool/roman_island_medium_{0}/roman_island_medium_{0}.a7m", 8),
            new(Anno117StaticAssets.RomanRegion, IslandSize.Large, "data/base/provinces/roman/islands/pool/roman_island_large_{0}/roman_island_large_{0}.a7m", new int[] {1, 2, 3, 4, 5, 6, 7, 9}),
            new(Anno117StaticAssets.RomanRegion, IslandSize.ExtraLarge, "data/base/provinces/roman/islands/pool/roman_island_extralarge_{0}/roman_island_extralarge_{0}.a7m", 4),
            
            // Celtic
            new(Anno117StaticAssets.CelticRegion, IslandSize.Small, "data/base/provinces/celtic/islands/pool/celtic_island_small_{0}/celtic_island_small_{0}.a7m", 7),
            new(Anno117StaticAssets.CelticRegion, IslandSize.Medium, "data/base/provinces/celtic/islands/pool/celtic_island_medium_{0}/celtic_island_medium_{0}.a7m", 7),
            new(Anno117StaticAssets.CelticRegion, IslandSize.Large, "data/base/provinces/celtic/islands/pool/celtic_island_large_{0}/celtic_island_large_{0}.a7m", 8)
        };
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
        public static RegionAsset? RomanRegion { get; private set; }

        [StaticAsset(RegionCelticGuid)]
        public static RegionAsset? CelticRegion { get; private set; }
        
        // Session Assets
        [StaticAsset(SessionLatiumGuid)]
        public static SessionAsset? LatiumSession { get; private set; }
        
        [StaticAsset(SessionAlbionGuid)]
        public static SessionAsset? AlbionSession { get; private set; }
        
        public override IEnumerable<RegionAsset?> SupportedRegions => new [] { RomanRegion, CelticRegion };
        public override IEnumerable<SessionAsset?> SupportedSessions => new[] { LatiumSession, AlbionSession };
        public override IEnumerable<SlotAsset?> SupportedSlots => Enumerable.Empty<SlotAsset>(); // Anno 117 does not use slots.
        public override IEnumerable<Type> SupportedAssetTypes => new[]
        {
            typeof(RegionAsset),
            typeof(FertilityAsset),
            typeof(RandomIslandAsset),
            typeof(MinimapSceneAsset),
            typeof(SessionAsset),
            typeof(MapTemplateAsset)
        };
    }
}