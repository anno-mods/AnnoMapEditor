using System.Collections.Generic;

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

        private const string Anno1800IconGeometry =
            "M4,18V20H8V18H4M4,14V16H14V14H4M10,18V20H14V18H10M16,14V16H20V14H16M16,18V20H20V18H16M2,22V8L7,12V8L12,12V8L17,12L18,2H21L22,12V22H2Z";

        private const string Anno117IconGeometry =
            "M6,5H18A1,1 0 0,1 19,6A1,1 0 0,1 18,7H6A1,1 0 0,1 5,6A1,1 0 0,1 6,5M21,2V4H3V2H21M15,8H17V22H15V8M7,8H9V22H7V8M11,8H13V22H11V8Z";

        private const string UnsupportedGameIconGeometry =
            "M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z";
            
            
        public static readonly Game Anno1800 = new(
            "Anno 1800", 
            Anno1800IconGeometry
            );
        
        public static readonly Game Anno117 = new(
            "Anno 117 - Pax Romana", 
            Anno117IconGeometry
            );

        public static readonly Game UnsupportedGame = new(
            "Unsupported Game",
            UnsupportedGameIconGeometry
            );
        
        public static IEnumerable<Game> SupportedGames => new[] { Anno1800, Anno117 };
        
        public string Title { get; }
        
        public string IconGeometry { get; }

        public string Path
        {
            get => _path ?? Title;
            private set => _path = value;
        }
        private string? _path;

        private Game(string title, string iconGeometry)
        {
            Title = title;
            IconGeometry = iconGeometry;
        }
    }
}