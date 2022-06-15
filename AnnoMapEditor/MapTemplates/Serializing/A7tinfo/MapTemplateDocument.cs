using System.Diagnostics.CodeAnalysis;
using FileDBSerializing.EncodingAwareStrings;
using FileDBSerializing.ObjectSerializer;

/*
 * Some properties don't follow naming conventions as they are defined by the file format.
 * [SuppressMessage("Style", "IDE1006:Naming Styles")]
 */

namespace AnnoMapEditor.MapTemplates.Serializing.A7tinfo
{
    public class MapTemplateDocument
    {
        public MapTemplate? MapTemplate { get; set; }
    }

    public class MapTemplate
    {
        public int[]? Size { get; set; }
        public int[]? PlayableArea { get; set; }
        public Empty? RandomlyPlacedThirdParties { get; set; }
        public int? ElementCount { get; set; }

        [FlatArray]
        public TemplateElement[]? TemplateElement { get; set; }
    }

    public class Empty
    {
    }

    public class TemplateElement
    {
        public int? ElementType { get; set; }
        public Element? Element { get; set; }
    }

    public class Element
    {
        // ElementType=0,1,2
        public int[]? Position { get; set; }

        // ElementType=0
        public UnicodeString? MapFilePath { get; set; }
        public byte? Rotation90 { get; set; }
        public UTF8String? IslandLabel { get; set; }
        public int[]? FertilityGuids { get; set; }
        public bool? RandomizeFertilities { get; set; }
        public int[][]? MineSlotMapping { get; set; }
        public RandomIslandConfig? RandomIslandConfig { get; set; }

        // ElementType=1
        public short? Size { get; set; }
        public Difficulty? Difficulty { get; set; }
        public Config? Config { get; set; }
    }

    public class RandomIslandConfig
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public Config? value { get; set; }
    }

    public class Config
    {
        public IslandType? Type { get; set; }
        public Difficulty? Difficulty { get; set; }
    }

    public class IslandType
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public short? id { get; set; }
    }

    public class Difficulty
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public short? id { get; set; }
    }
}
