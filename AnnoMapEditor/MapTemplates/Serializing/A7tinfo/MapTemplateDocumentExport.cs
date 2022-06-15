using FileDBSerializing.EncodingAwareStrings;
using FileDBSerializing.ObjectSerializer;
using System.Collections.Generic;
using System.Linq;

/*
 * Special export classes to make sure tags are different per ElementType.
 */

namespace AnnoMapEditor.MapTemplates.Serializing.A7tinfo
{
    public class MapTemplateDocumentExport
    {
        public MapTemplateExport? MapTemplate { get; set; }
    }

    public class MapTemplateExport
    {
        public int[]? Size { get; }
        public int[]? PlayableArea { get; }
        public Empty? RandomlyPlacedThirdParties { get; }
        public int? ElementCount { get; }

        [FlatArray]
        public TemplateElementExport[]? TemplateElement { get; }

        public MapTemplateExport(MapTemplate template, IEnumerable<TemplateElement> elements)
        {
            Size = template.Size;
            PlayableArea = template.PlayableArea;
            RandomlyPlacedThirdParties = template.RandomlyPlacedThirdParties;
            TemplateElement = elements.Select(x => TemplateElementExport.FromTemplateElement(x)).ToArray();
            ElementCount = TemplateElement.Length;

        }
    }

    public class TemplateElementExport
    {
        public static TemplateElementExport FromTemplateElement(TemplateElement element)
        {
            if (element.ElementType == 2)
            {
                return new TemplateElement2(new Element2()
                {
                    Position = element.Element?.Position
                });
            }
            else if (element.ElementType == 1)
            {
                return new TemplateElement1(new Element1()
                {
                    Position = element.Element?.Position,
                    Size = element.Element?.Size,
                    Difficulty = element.Element?.Difficulty,
                    Config = element.Element?.Config
                });
            }
            else
            {
                return new TemplateElement0(new Element0()
                {
                    Position = element.Element?.Position,
                    MapFilePath = element.Element?.MapFilePath,
                    Rotation90 = element.Element?.Rotation90,
                    IslandLabel = element.Element?.IslandLabel,
                    FertilityGuids = element.Element?.FertilityGuids,
                    RandomizeFertilities = element.Element?.RandomizeFertilities,
                    MineSlotMapping = element.Element?.MineSlotMapping,
                    RandomIslandConfig = element.Element?.RandomIslandConfig
                });
            }
        }
    }

    public class TemplateElement0 : TemplateElementExport
    {
        public ElementExport Element { get; set; }

        public TemplateElement0(ElementExport element)
        {
            Element = element;
        }
    }

    public class TemplateElement1 : TemplateElementExport
    {
        public int ElementType { get; } = 1;
        public ElementExport Element { get; set; }

        public TemplateElement1(ElementExport element)
        {
            Element = element;
        }
    }

    public class TemplateElement2 : TemplateElementExport
    {
        public int ElementType { get; } = 2;
        public ElementExport Element { get; set; }

        public TemplateElement2(ElementExport element)
        {
            Element = element;
        }
    }

    public class ElementExport
    {
        //public int[]? Position { get; set; }
    }

    public class Element0 : ElementExport
    {
        public int[]? Position { get; set; }
        public UnicodeString? MapFilePath { get; set; }
        public byte? Rotation90 { get; set; }
        public UTF8String? IslandLabel { get; set; }
        public int[]? FertilityGuids { get; set; }
        public bool? RandomizeFertilities { get; set; }
        public int[][]? MineSlotMapping { get; set; }
        public RandomIslandConfig? RandomIslandConfig { get; set; }
    }

    public class Element1 : ElementExport
    {
        public int[]? Position { get; set; }
        public short? Size { get; set; }
        public Difficulty? Difficulty { get; set; }
        public Config? Config { get; set; }
    }

    public class Element2 : ElementExport
    {
        public int[]? Position { get; set; }
    }
}
