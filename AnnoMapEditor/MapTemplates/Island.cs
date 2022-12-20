using Anno.FileDBModels.Anno1800.MapTemplate;
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

    public class Island : MapElement
    {
        #region template data
        public int ElementType { get; set; } = 0;
        private IslandSize _size = IslandSize.Small;
        public IslandSize Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(IsStarter));
                }

                // always set size to ensure IsDefault is correct
                // TODO != should consider IsDefault + add method of for same size
                _size = value;
            }
        }


        private IslandType _type = IslandType.Normal;
        public IslandType Type 
        {
            get => _type;
            set
            {
                if (value != _type)
                {
                    _type = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStarter));
                }
            }
        }
        public string? Label { get; set; }
        #endregion
        private IslandMap? _islandMap = null;
        public IslandMap? IslandMap
        {
            get => _islandMap;
            set
            {
                if (value != _islandMap)
                {
                    _islandMap = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Use the IslandMap's size if it has one. Otherwise default to the IslandSize's size.
        /// </summary>
        public override int SizeInTiles => _islandMap?.SizeInTiles != null ? (int) _islandMap!.SizeInTiles : Size.InTiles;

        public bool IsPool => _islandMap == null;
        public bool IsNew => _template is null;

        // TODO create view model of islands
        public bool IsStarter
        { 
            get => Type == IslandType.Starter;
            set
            {
                if (value != IsStarter)
                {
                    Type = value ? IslandType.Starter : IslandType.Normal;
                }
            }
        }

        private TemplateElement? _template;

        private Region _region;


        public Island(Region region, Vector2 position)
        {
            _region = region;
            Position = position;
        }

        public Island Clone()
        {
            var island = new Island(_region, Position)
            {
                ElementType = ElementType,
                Size = Size,
                Type = Type,
                Rotation = Rotation,
                IslandMap = IslandMap,
                Label = Label,
                _template = _template
            };

            island.Init();
            return island;
        }

        public static Island FromSerialized(TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;
            Vector2 position = new(element?.Position);
            IslandSize islandSize = new IslandSize(element?.Size);
            string? mapFilePath = element?.MapFilePath?.ToString();
            IslandMap? islandMap = mapFilePath != null ? region.IslandMapPools[islandSize].GetFromPath(mapFilePath!) : null;

            var island = new Island(region, position)
            {
                ElementType = templateElement.ElementType ?? 0,
                Size = islandSize,
                Type = new IslandType(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                IslandMap = islandMap,
                Label = element?.IslandLabel?.ToString(),
                _template = templateElement
            };

            island.Init();
            return island;
        }

        public static Island Create(IslandSize size, IslandType type, Vector2 position)
        {
            var island = new Island(Region.Moderate, position)
            {
                ElementType = 1,
                Size = size,
                Type = type
            };
            return island;
        }

        public void CreateTemplate()
        {
            _template = new TemplateElement
            {
                Element = new Element()
            };
            OnPropertyChanged();
        }

        public override TemplateElement ToTemplate()
        {
            TemplateElement templateElement = new TemplateElement();

            if (_template?.Element is null)
                return templateElement;

            if (ElementType != 0)
            {
                templateElement.ElementType = ElementType;
            }

            //Create a fully new templateElement for export, so unwanted values are clean
            templateElement.Element = new Element();
            switch (ElementType)
            {
                //Starting spot
                case 2:
                    throw new Exception($"Using Island instead of StartingSpot!");

                //Pool island
                case 1:
                    templateElement.Element.Position = new int[] { Position.X, Position.Y };
                    templateElement.Element.Size = Size.ElementValue;
                    templateElement.Element.Difficulty = new Difficulty();
                    templateElement.Element.Config = new Config()
                    {
                        Type = new() { id = Type.ElementValue != 0 ? Type.ElementValue : null },
                        Difficulty = new()
                    };
                    break;
                //Fixed island
                default:
                    //Fixed island without MapPath is impossible
                    if (_islandMap == null)
                        return null;

                    templateElement.Element.Position = new int[] { Position.X, Position.Y };
                    templateElement.Element.MapFilePath = _islandMap.FilePath;
                    templateElement.Element.Rotation90 = (byte)Rotation;

                    if(Label is not null)
                    {
                        templateElement.Element.IslandLabel = Label;
                    }

                    if(_template?.Element?.FertilityGuids is not null)
                    {
                        templateElement.Element.FertilityGuids = new int[_template.Element.FertilityGuids.Length];
                        Array.Copy(_template.Element.FertilityGuids, templateElement.Element.FertilityGuids, _template.Element.FertilityGuids.Length);
                    }
                    else
                    {
                        templateElement.Element.FertilityGuids = new int[0];
                    }

                    templateElement.Element.RandomizeFertilities = _template?.Element?.RandomizeFertilities;
                    templateElement.Element.MineSlotMapping = new List<Tuple<long, int>>( 
                        _template?.Element?.MineSlotMapping?.Select(
                            x => new Tuple<long, int>(x.Item1, x.Item2)
                        ) ?? Enumerable.Empty<Tuple<long, int>>()
                    );

                    templateElement.Element.RandomIslandConfig = new RandomIslandConfig()
                    {
                        value = new Config()
                        {
                            Type = new() { id = Type.ElementValue != 0 ? Type.ElementValue : null },
                            Difficulty = new() { id = _template?.Element?.RandomIslandConfig?.value?.Difficulty?.id }
                        }
                    };
                    break;
            }

            return templateElement;
        }

        public void RandomizeIslandMap()
        {
            IslandMap = _region.IslandMapPools[Size].GetRandomIslandMap(); // TODO: Refactor to use IslandMap
            Rotation = Random.Shared.Next(0, 3);

            if (IslandMap.FilePath.Contains("_l_") && Size.IsDefault)
                Size = IslandSize.Large;
            else if (IslandMap.FilePath.Contains("_m_") && Size.IsDefault)
                Size = IslandSize.Medium;

            OnPropertyChanged();
        }

        public void SetRegion(Region region)
        {
            _region = region;
            RandomizeIslandMap();
        }

        public void Init()
        {
            if (_islandMap == null)
                RandomizeIslandMap();

//            string? mapPath = _islandMap.FilePath;
//            if (mapPath is not null && SpecialIslands.CachedSizes.ContainsKey(mapPath))
//                SizeInTiles = SpecialIslands.CachedSizes[mapPath];
        }
    }
}
