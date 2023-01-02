using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Utilities;
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
        public MapElementType ElementType
        { 
            get => _elementType;
            set => SetProperty(ref _elementType, value);
        }
        private MapElementType _elementType = MapElementType.PoolIsland;

        public Vector2 Position 
        { 
            get => _position;
            set => SetProperty(ref _position, value);
        }
        private Vector2 _position = Vector2.Zero;

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
        private IslandSize _size = IslandSize.Small;

        public int SizeInTiles => (IsPool || MapSizeInTiles == 0) ? Size.DefaultSizeInTiles : MapSizeInTiles;

        public IslandType Type 
        {
            get => _type;
            set => SetProperty(ref _type, value, dependendProperties: new[] { nameof(IsStarter) });
        }
        private IslandType _type = IslandType.Normal;

        public string? ImageFile 
        { 
            get => _imageFile;
            set => SetProperty(ref _imageFile, value);
        }
        private string? _imageFile;

        public int Rotation 
        { 
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        } 
        private int _rotation = 0;

        public string? MapPath 
        { 
            get => _mapPath;
            set => SetProperty(ref _mapPath, value);
        }
        private string? _mapPath;

        public int MapSizeInTiles 
        {
            get => _mapSizeInTiles;
            private set => SetProperty(ref _mapSizeInTiles, value);
        }
        private int _mapSizeInTiles;

        public string? Label 
        { 
            get => _label;
            set => SetProperty(ref _label, value); 
        }
        private string? _label;

        public string? AssumedMapPath
        { 
            get => _assumedMapPath;
            private set => SetProperty(ref _assumedMapPath, value); 
        }
        private string? _assumedMapPath;

        public bool IsPool => !IsStartingSpot && string.IsNullOrEmpty(MapPath);
        public bool IsStartingSpot => ElementType == MapElementType.StartingSpot;
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

        public TemplateElement? Template
        {
            get => _template;
            set => SetProperty(ref _template, value, dependendProperties: new[] { nameof(IsNew) });
        }
        private TemplateElement? _template;

        private Region _region { get; }

        public int Counter { get; set; }


        public Island(Region region)
        {
            _region = region;
        }


        public static async Task<Island> FromSerialized(TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;

            var island = new Island(region)
            {
                ElementType = (MapElementType) (templateElement.ElementType ?? 0),
                Position = new Vector2(element?.Position),
                Size = IslandSize.FromElementValue(element?.Size),
                Type = IslandType.FromElementValue(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                MapPath = element?.MapFilePath?.ToString(),
                Label = element?.IslandLabel?.ToString(),
                _template = templateElement
            };

            await island.InitAsync();
            return island;
        }

        public void CreateTemplate()
        {
            Template = new TemplateElement
            {
                Element = new Element()
            };
        }

        public TemplateElement? ToTemplate()
        {
            TemplateElement templateElement = new TemplateElement();

            if (_template?.Element is null)
                return templateElement;

            if (ElementType != MapElementType.FixedIsland)
            {
                templateElement.ElementType = (short) ElementType;
            }

            //Create a fully new templateElement for export, so unwanted values are clean
            templateElement.Element = new Element();

            // The editor's coordinate system's axis are flipped compared to Anno1800. Thus we must
            // flip X and Y when serializing.
            templateElement.Element.Position = new int[] { this.Position.Y, this.Position.X };

            switch (ElementType)
            {
                //Starting spot
                case MapElementType.StartingSpot:
                    break;

                //Pool island
                case MapElementType.PoolIsland:
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

            // fallback to read out map file
            int sizeInTiles = await IslandReader.ReadTileInSizeFromFileAsync(AssumedMapPath);
            if (sizeInTiles != 0)
                MapSizeInTiles = sizeInTiles;

            if (Settings.Instance.DataPath is not null)
            {
                string activeMapImagePath = Path.Combine(Path.GetDirectoryName(AssumedMapPath) ?? "", "_gamedata", Path.GetFileNameWithoutExtension(AssumedMapPath), "mapimage.png");
                ImageFile = activeMapImagePath;
            }
        }

        public async Task InitAsync()
        {
            if (IsStartingSpot)
            {
                MapSizeInTiles = Vector2.Tile.X;
                return;
            }

            string? mapPath = MapPath;
            if (mapPath is not null && SpecialIslands.CachedSizes.ContainsKey(mapPath))
                MapSizeInTiles = SpecialIslands.CachedSizes[mapPath];

            if (mapPath == null)
            {
                mapPath = Pool.GetRandomIslandPath(_region, Size);
                Rotation = Random.Shared.Next(0, 3);
            }

            if (mapPath != null && Size == IslandSize.Default)
            {
                if (mapPath.Contains("_l_"))
                    Size = IslandSize.Large;
                else if (mapPath.Contains("_m_"))
                    Size = IslandSize.Medium;
            }

            AssumedMapPath = mapPath;
            await UpdateExternalDataAsync();
        }
    }
}
