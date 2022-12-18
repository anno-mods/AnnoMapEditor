using Anno_FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Islands;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    static class SpecialIslands
    {
        public static readonly Dictionary<string, int> CachedSizes = new Dictionary<string, int>()
        {
            ["data/dlc01/sessions/islands/pool/moderate_c_01/moderate_c_01.a7m"] = 768,
            ["data/dlc01/sessions/islands/pool/moderate_3rdparty06_01/moderate_3rdparty06_01.a7m"] = 128,
            ["data/dlc01/sessions/islands/pool/moderate_dst_04/moderate_dst_04.a7m"] = 192,
            ["data/sessions/islands/pool/moderate/community_island/community_island.a7m"] = 384,
            ["data/dlc01/sessions/islands/pool/moderate_encounter_01/moderate_encounter_01.a7m"] = 256
        };
    }

    public class Island : ObservableBase
    {
        #region template data
        public int ElementType { get; set; } = 0;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public IslandSize Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    IslandChanged?.Invoke();
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(IsStarter));
                }

                // always set size to ensure IsDefault is correct
                // TODO != should consider IsDefault + add method of for same size
                _size = value;
            }
        }
        private IslandSize _size = IslandSize.Small;
        public IslandMap? IslandMap { get; set; } // TODO: Make non-nullable
        public int SizeInTiles => (IsPool || MapSizeInTiles == 0) ? Size.InTiles : MapSizeInTiles;
        public IslandType Type { get; set; } = IslandType.Normal;
        public bool Hide { get; set; } = false;
        public string? ImageFile { get; set; }
        public int Rotation { get; set; } = 0;
        public int MapSizeInTiles => IslandMap?.SizeInTiles ?? Vector2.Tile.X; // TODO: Remove when extracting StartingPoint
        public string? Label { get; set; }
        #endregion

        public string? AssumedMapPath { get; private set; }
        public bool IsPool => ElementType != 2 && IslandMap == null;
        public bool IsStartingSpot => ElementType == 2;
        public bool IsNew => template is null;

        // TODO create view model of islands
        public bool IsStarter
        { 
            get => Type == IslandType.Starter;
            set
            {
                if (value != IsStarter)
                {
                    Type = value ? IslandType.Starter : IslandType.Normal;
                    IslandChanged?.Invoke();
                }
            }
        }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private TemplateElement? template;

        private Region Region { get; }
        public int Counter { get; set; }

        public delegate void IslandChangedHandler();
        public event IslandChangedHandler? IslandChanged;

        private Island(Region region)
        {
            Region = region;
        }

        public Island Clone()
        {
            var island = new Island(Region)
            {
                ElementType = ElementType,
                Position = Position,
                Size = Size,
                Type = Type,
                Rotation = Rotation,
                IslandMap = IslandMap,
                Label = Label,
                template = template
            };

            island.Init(Region);
            return island;
        }

        public static Island FromSerialized(TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;
            var islandSize = new IslandSize(element?.Size);
            var islandMap = region.IslandMapPools[islandSize].GetFromFilePath(element?.MapFilePath?.ToString());

            var island = new Island(region)
            {
                ElementType = templateElement.ElementType ?? 0,
                Position = new Vector2(element?.Position),
                Size = islandSize,
                Type = new IslandType(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                IslandMap = islandMap,
                Label = element?.IslandLabel?.ToString(),
                template = templateElement
            };

            island.Init(region);
            return island;
        }

        public static Island Create(IslandSize size, IslandType type, Vector2 position)
        {
            var island = new Island(Region.Moderate)
            {
                ElementType = 1,
                Position = position,
                Size = size,
                Type = type
            };
            return island;
        }

        public static List<Island> CreateNewStartingSpots(int playableSize, int margin, Region region)
        {
            const int SPACING = 64;

            List<Island> starts = new List<Island>()
            {
                CreateStartingSpot(region, margin + SPACING, playableSize + margin - SPACING),
                CreateStartingSpot(region, margin + SPACING, playableSize + margin - 2*SPACING),
                CreateStartingSpot(region, margin + 2*SPACING, playableSize + margin - SPACING),
                CreateStartingSpot(region, margin + 2*SPACING, playableSize + margin - 2*SPACING),
            };

            return starts;


        }

        private static Island CreateStartingSpot(Region region, int x, int y)
        {
            return new Island(region)
            {
                ElementType = 2,
                Position = new Vector2(x, y)
            };
        }

        public void CreateTemplate()
        {
            template = new TemplateElement
            {
                Element = new Element()
            };
            IslandChanged?.Invoke();
        }

        public TemplateElement? ToTemplate()
        {
            TemplateElement templateElement = new TemplateElement();

            if (template?.Element is null)
                return templateElement;

            if (this.ElementType != 0)
            {
                templateElement.ElementType = this.ElementType;
            }

            //Create a fully new templateElement for export, so unwanted values are clean
            templateElement.Element = new Element();
            switch (this.ElementType)
            {
                //Starting spot
                case 2:
                    templateElement.Element.Position = new int[] { this.Position.X, this.Position.Y };
                    break;
                //Pool island
                case 1:
                    templateElement.Element.Position = new int[] { this.Position.X, this.Position.Y };
                    templateElement.Element.Size = this.Size.ElementValue;
                    templateElement.Element.Difficulty = new Difficulty();
                    templateElement.Element.Config = new Config()
                    {
                        Type = new() { id = this.Type.ElementValue != 0 ? this.Type.ElementValue : null },
                        Difficulty = new()
                    };
                    break;
                //Fixed island
                default:
                    //Fixed island without MapPath is impossible
                    if (IslandMap == null)
                        return null;

                    templateElement.Element.Position = new int[] { this.Position.X, this.Position.Y };
                    templateElement.Element.MapFilePath = IslandMap.FilePath;
                    templateElement.Element.Rotation90 = (byte)this.Rotation;

                    if(Label is not null)
                    {
                        templateElement.Element.IslandLabel = Label;
                    }

                    if(template?.Element?.FertilityGuids is not null)
                    {
                        templateElement.Element.FertilityGuids = new int[template.Element.FertilityGuids.Length];
                        Array.Copy(this.template.Element.FertilityGuids, templateElement.Element.FertilityGuids, template.Element.FertilityGuids.Length);
                    }
                    else
                    {
                        templateElement.Element.FertilityGuids = new int[0];
                    }

                    templateElement.Element.RandomizeFertilities = template?.Element?.RandomizeFertilities;
                    templateElement.Element.MineSlotMapping = new List<Tuple<long, int>>( 
                        template?.Element?.MineSlotMapping?.Select(
                            x => new Tuple<long, int>(x.Item1, x.Item2)
                        ) ?? Enumerable.Empty<Tuple<long, int>>()
                    );

                    templateElement.Element.RandomIslandConfig = new RandomIslandConfig()
                    {
                        value = new Config()
                        {
                            Type = new() { id = this.Type.ElementValue != 0 ? this.Type.ElementValue : null },
                            Difficulty = new() { id = template?.Element?.RandomIslandConfig?.value?.Difficulty?.id }
                        }
                    };
                    break;
            }

            return templateElement;
        }

        public void UpdateExternalData()
        {
            if (AssumedMapPath is null)
                return;

            if (Settings.Instance.DataPath is not null)
            {
                string activeMapImagePath = Path.Combine(Path.GetDirectoryName(AssumedMapPath) ?? "", "_gamedata", Path.GetFileNameWithoutExtension(AssumedMapPath), "mapimage.png");
                ImageFile = activeMapImagePath;
            }
            IslandChanged?.Invoke();
        }

        public void RandomizeMap()
        {
            IslandMap = Region.IslandMapPools[Size].GetRandomMap();
            Rotation = rnd.Next(0, 3);
        }

        public void Init(Region region)
        {
            if (ElementType == 2)
            {
                return;
            }

            if (IslandMap == null)
            {
                IslandMap = region.IslandMapPools[Size].GetRandomMap();
                Rotation = rnd.Next(0, 3);
            }

            if (IslandMap.FilePath.Contains("_l_") && Size.IsDefault)
                Size = IslandSize.Large;
            else if (IslandMap.FilePath.Contains("_m_") && Size.IsDefault)
                Size = IslandSize.Medium;

            AssumedMapPath = IslandMap.FilePath;
            UpdateExternalData();
        }
    }
}
