using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI;
using System;
using System.Collections.Generic;
using System.IO;
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
        public int ElementType { get; set; } = 0;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public IslandSize Size { get; set; } = IslandSize.Small;
        public int SizeInTiles => (IsPool || MapSizeInTiles == 0) ? Size.InTiles : MapSizeInTiles;
        public IslandType Type { get; set; } = IslandType.Normal;
        public bool Hide { get; set; } = false;
        public string? ImageFile { get; set; }
        public int Rotation { get; set; } = 0;
        public string? MapPath { get; set; }
        public int MapSizeInTiles { get; private set; }
        public string? AssumedMapPath { get; private set; }
        public bool IsPool => ElementType != 2 && string.IsNullOrEmpty(MapPath);
        public string? Label { get; set; }

        public bool IsNew => template is null;

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private Serializing.A7tinfo.TemplateElement? template;

        private Region Region { get; }

        public delegate void IslandChangedHandler();
        public event IslandChangedHandler? IslandChanged;

        private Island(Region region)
        {
            Region = region;
        }

        public async Task<Island> CloneAsync()
        {
            var island = new Island(Region)
            {
                ElementType = ElementType,
                Position = Position,
                Size = Size,
                Type = Type,
                Rotation = Rotation,
                MapPath = MapPath,
                Label = Label,
                template = template
            };

            await island.InitAsync(Region);
            return island;
        }

        public static async Task<Island> FromSerialized(Serializing.A7tinfo.TemplateElement templateElement, Region region)
        {
            var element = templateElement.Element;
            var island = new Island(region)
            {
                ElementType = templateElement.ElementType ?? 0,
                Position = new Vector2(element?.Position),
                Size = new IslandSize(element?.Size),
                Type = new IslandType(element?.RandomIslandConfig?.value?.Type?.id ?? element?.Config?.Type?.id),
                Rotation = Math.Clamp((int?)element?.Rotation90 ?? 0, 0, 3),
                MapPath = element?.MapFilePath?.ToString(),
                Label = element?.IslandLabel?.ToString(),
                template = templateElement
            };

            await island.InitAsync(region);
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
            const int SPACING = 32;

            List<Island> starts = new List<Island>()
            {
                CreateStartingSpot(region, margin + SPACING, playableSize + margin - SPACING),
                CreateStartingSpot(region, margin + SPACING, playableSize + margin - 2* SPACING),
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
            template = new Serializing.A7tinfo.TemplateElement
            {
                Element = new Serializing.A7tinfo.Element()
            };
            IslandChanged?.Invoke();
        }

        public Serializing.A7tinfo.TemplateElement? ToTemplate()
        {
            if (template?.Element is null)
                return template;

            template.ElementType = ElementType;
            template.Element.Position = new int[] { Position.X, Position.Y };
            template.Element.Size = Size.ElementValue;
            if (IsPool)
            {
                template.Element.Config = new()
                {
                    Type = Type.ElementValue != 0 ? new() { id = Type.ElementValue } : null
                };
                template.Element.RandomIslandConfig = null;
                template.Element.MapFilePath = null;
            }
            else
            {

            }
            return template;
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
            IslandChanged?.Invoke();
        }

        public async Task UpdateAsync()
        {
            await InitAsync(Region);
            IslandChanged?.Invoke();
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
                mapPath = region.GetRandomIslandPath(Size);
                Rotation = rnd.Next(0, 3);
            }

            if (mapPath != null)
            {
                //if (mapPath.Contains("_dst_"))
                //    Hide = true;
                // else
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
