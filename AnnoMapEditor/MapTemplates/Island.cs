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
        public int SizeInTiles => (IsPool || MapSizeInTiles == 0) ? Size.InTiles : MapSizeInTiles;

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
        private string? _imageFile;
        public string? ImageFile 
        {
            get => _imageFile;
            set
            {
                if (value != _imageFile)
                {
                    _imageFile = value;
                    OnPropertyChanged();
                }
            }
        }
        public string? MapPath { get; set; }
        public string? Label { get; set; }
        #endregion
        public string? AssumedMapPath { get; private set; }
        public bool IsPool => ElementType != 2 && string.IsNullOrEmpty(MapPath);
        public bool IsStartingSpot => ElementType == 2;
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

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private TemplateElement? _template;

        private Region Region { get; }
        public int Counter { get; set; }

        public Island(Region region, Vector2 position)
        {
            Region = region;
            Position = position;
        }

        public async Task<Island> CloneAsync()
        {
            var island = new Island(Region, Position)
            {
                ElementType = ElementType,
                Size = Size,
                Type = Type,
                Rotation = Rotation,
                MapPath = MapPath,
                Label = Label,
                _template = _template
            };

            await island.InitAsync(Region);
            return island;
        }

        public static async Task<Island> FromSerialized(TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;
            Vector2 position = new Vector2(element?.Position);

            var island = new Island(region, position)
            {
                ElementType = templateElement.ElementType ?? 0,
                Size = new IslandSize(element?.Size),
                Type = new IslandType(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                MapPath = element?.MapFilePath?.ToString(),
                Label = element?.IslandLabel?.ToString(),
                _template = templateElement
            };

            await island.InitAsync(region);
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
                    throw new Exception($"Using Island instead of StartingSpot!");
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
                    if (MapPath == null)
                        return null;

                    templateElement.Element.Position = new int[] { this.Position.X, this.Position.Y };
                    templateElement.Element.MapFilePath = MapPath;
                    templateElement.Element.Rotation90 = (byte)this.Rotation;

                    if(Label is not null)
                    {
                        templateElement.Element.IslandLabel = Label;
                    }

                    if(_template?.Element?.FertilityGuids is not null)
                    {
                        templateElement.Element.FertilityGuids = new int[_template.Element.FertilityGuids.Length];
                        Array.Copy(this._template.Element.FertilityGuids, templateElement.Element.FertilityGuids, _template.Element.FertilityGuids.Length);
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
                            Type = new() { id = this.Type.ElementValue != 0 ? this.Type.ElementValue : null },
                            Difficulty = new() { id = _template?.Element?.RandomIslandConfig?.value?.Difficulty?.id }
                        }
                    };
                    break;
            }

            return templateElement;
        }

        public async Task UpdateExternalDataAsync()
        {
            if (AssumedMapPath is null)
                return;

            if (Settings.Instance.DataPath is not null)
            {
                string activeMapImagePath = Path.Combine(Path.GetDirectoryName(AssumedMapPath) ?? "", "_gamedata", Path.GetFileNameWithoutExtension(AssumedMapPath), "mapimage.png");
                ImageFile = activeMapImagePath;
            }
        }

        public async Task UpdateAsync()
        {
            await InitAsync(Region);
        }

        public async Task InitAsync(Region region)
        {
            if (ElementType == 2)
            {
                MapSizeInTiles = Vector2.Tile.X;
                return;
            }

            string? mapPath = MapPath;
            if (mapPath is not null && SpecialIslands.CachedSizes.ContainsKey(mapPath))
                MapSizeInTiles = SpecialIslands.CachedSizes[mapPath];

            if (mapPath == null)
            {
                mapPath = region.IslandMapPools[Size].GetRandomIslandPath(); // TODO: Refactor to use IslandMap
                Rotation = rnd.Next(0, 3);
            }

            if (mapPath != null)
            {
                if (mapPath.Contains("_l_") && Size.IsDefault)
                    Size = IslandSize.Large;
                else if (mapPath.Contains("_m_") && Size.IsDefault)
                    Size = IslandSize.Medium;
            }

            AssumedMapPath = mapPath;
            await UpdateExternalDataAsync();
        }
    }
}
