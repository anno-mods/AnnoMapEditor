using System;
using System.Collections.Generic;
using AnnoMapEditor.DataArchives.Assets.Models;

namespace AnnoMapEditor.DataArchives.Games
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
    
    public abstract class Game {
        
        public static Game Anno1800 => new Anno1800Game();
        public static Game Anno117 => new Anno117Game();
        public static Game UnsupportedAnno => new UnsupportedGame();
        public static IEnumerable<Game> SupportedGames => new[] { Anno1800, Anno117 };
        
        public abstract string Title { get; }
        
        /**
         * This is not the entire Game Path, but the part that is searched for to identify the game. Defaults to the
         * games Title.
         */
        public virtual string Path => Title;
        
        /**
         * The Icon representation of each game uses a canvas XAML object. The IconGeometry is the vector data to be
         * used for that Canvas.
         */
        public abstract string IconGeometry { get; }
        public virtual string? AssetsXmlPath => null;
        public virtual StaticGameAssets? StaticAssets => null;

        private class UnsupportedGame : Game
        {
            public override string Title => "Unsupported Game";
            public override string IconGeometry => "M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z";
        }
    }

    /*
     * TODO: For more flexibility and possible future modded data support, using static assets could be a problem.
     * Ideally, all assets should be loaded dynamically. But this would probably require quite a lot of work.
     * This might be an idea for a future version after compatibility with 117 is archived. 
     */
    
    // TODO: Add all remaining kinds of static assets for each game, then remove static assets from the respective asset classes.
    
    public abstract class StaticGameAssets
    {
        public abstract IEnumerable<RegionAsset?> SupportedRegions { get; }
        public abstract IEnumerable<SessionAsset?> SupportedSessions { get; }
        public abstract IEnumerable<SlotAsset?> SupportedSlots { get; }
        public abstract IEnumerable<Type> SupportedAssetTypes { get; }
    }
}