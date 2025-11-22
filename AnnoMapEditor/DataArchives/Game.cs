using System;
using System.Collections.Generic;
using System.Linq;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;

namespace AnnoMapEditor.DataArchives
{
    public class Game
    {
        /*
         * Symbols taken from https://pictogrammers.com/library/mdi/
         * 
         * Download type: XAML (Canvas)
         * Copied the "Data" to the corresponding iconGeometry parameter of each game.
         *
         * Icon Names:
         *      Anno 1800:               Factory
         *      Anno 117 - Pax Romana:   Pillar
         *      Unsupported Game:        Help
         */

        private const string Anno1800Title = "Anno 1800";
        private const string Anno1800IconGeometry = "M4,18V20H8V18H4M4,14V16H14V14H4M10,18V20H14V18H10M16,14V16H20V14H16M16,18V20H20V18H16M2,22V8L7,12V8L12,12V8L17,12L18,2H21L22,12V22H2Z";
        private const string Anno1800AssetsXmlPath = "data/config/export/main/asset/assets.xml";

        private const string Anno117Title = "Anno 117 - Pax Romana";
        private const string Anno117IconGeometry = "M6,5H18A1,1 0 0,1 19,6A1,1 0 0,1 18,7H6A1,1 0 0,1 5,6A1,1 0 0,1 6,5M21,2V4H3V2H21M15,8H17V22H15V8M7,8H9V22H7V8M11,8H13V22H11V8Z";
        private const string Anno117AssetsXmlPath = "data/base/config/export/assets.xml";

        private const string UnsupportedGameIconGeometry = "M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z";
        
        public static readonly Game Anno1800 = new(Anno1800Title, Anno1800IconGeometry, Anno1800AssetsXmlPath, new Anno1800StaticAssets());
        public static readonly Game Anno117 = new( Anno117Title, Anno117IconGeometry, Anno117AssetsXmlPath, new Anno117StaticAssets());
        public static readonly Game UnsupportedGame = new();
        
        public static IEnumerable<Game> SupportedGames => new[] { Anno1800, Anno117 };
        
        public string Title { get; }
        
        /**
         * The Icon representation of each game uses a canvas XAML object. The IconGeometry is the vector data to be
         * used for that Canvas.
         */
        public string IconGeometry { get; }
        
        public string AssetsXmlPath { get; }

        public StaticGameAssets StaticAssets { get; }

        /**
         * This is not the entire Game Path, but the part that is searched for to identify the game. Defaults to the
         * games Title.
         */
        public string Path
        {
            get => _path ?? Title;
            private set => _path = value;
        }
        private string? _path;

        private Game(
            string title, 
            string iconGeometry,
            string assetsXmlPath,
            StaticGameAssets staticAssets
        ){
            Title = title;
            IconGeometry = iconGeometry;
            AssetsXmlPath = assetsXmlPath;
            StaticAssets = staticAssets;
        }

        /** Reduced constructor just fo the unsupported game. Never gets past start window, warning can be ignored. */
        private Game()
        {
            Title = "Unsupported Game";
            IconGeometry = UnsupportedGameIconGeometry;
        }
    }

    // TODO: For more flexibility and possible future modded data support, using static assets could be a problem.
    // Ideally, all assets should be loaded dynamically. But this would probably require quite a lot of work.
    // This might be an idea for a future version after compatibility with 117 is archived. 
    
    public abstract class StaticGameAssets
    {
        public abstract IEnumerable<RegionAsset?> SupportedRegions { get; }
        public abstract IEnumerable<SessionAsset?> SupportedSessions { get; }
        public abstract IEnumerable<SlotAsset?> SupportedSlots { get; }
        public abstract IEnumerable<Type> SupportedAssetTypes { get; }
    }
    
    // TODO: Add all remaining kinds of static assets for each game, then remove static assets from the respective asset classes.

    public class Anno1800StaticAssets : StaticGameAssets
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

    public class Anno117StaticAssets : StaticGameAssets
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