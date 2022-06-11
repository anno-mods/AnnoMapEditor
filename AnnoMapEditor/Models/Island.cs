using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

using FileDBSerializing;
using AnnoMapEditor.External;
using AnnoMapEditor.Utils;

namespace AnnoMapEditor.Models
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

    public class Island
    {
        public int ElementType { get; set; } = 0;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public IslandSize Size { get; set; } = IslandSize.Small;
        public int SizeInTiles { get; set; } = 0;
        public IslandType Type { get; set; } = IslandType.Normal;
        public bool Hide { get; set; } = false;
        public string? ImageFile { get; set; }
        public int Rotation { get; set; } = 0;
        public string? MapPath { get; private set; }
        public string? AssumedMapPath { get; private set; }
        public bool IsPool => string.IsNullOrEmpty(MapPath);
        public string? Label { get; set; }

        private static readonly Random rnd = new((int)DateTime.Now.Ticks);

        private Island() { }

        public static async Task<Island> FromXElement(XElement node, Region region)
        {
            var island = new Island()
            {
                ElementType = int.TryParse(node.GetValueFromPath("ElementType") ?? "0", out int b) ? b : 0,
                Position = new Vector2(node.GetValueFromPath("Position")),
                Size = new IslandSize(node.GetValueFromPath("Size")),
                Type = new IslandType(node.GetValueFromPath("RandomIslandConfig/value/Type/id") ?? node.GetValueFromPath("Config/Type/id")),
                Rotation = int.Parse(node.GetValueFromPath("Rotation90") ?? "0"),
                MapPath = node.GetValueFromPath("MapFilePath"),
                Label = node.GetValueFromPath("IslandLabel")
            };

            await island.InitAsync(region);
            return island;
        }

        public static async Task<Island> FromFileDBNode(FileDBNode node, Region region)
        {
            var randomType = node.GetBytesFromPath("RandomIslandConfig/value/Type/id");
            var island = new Island()
            {
                ElementType = node.Parent.GetBytesFromPath("ElementType")?[0] ?? 0,
                Position = new Vector2(node.GetBytesFromPath("Position")),
                Size = new IslandSize(node.GetBytesFromPath("Size")),
                Type = new IslandType(randomType ?? node.GetBytesFromPath("Config/Type/id"), randomType is not null),
                Rotation = node.GetBytesFromPath("Rotation90")?[0] ?? 0,
                MapPath = node.GetStringFromPath("MapFilePath", System.Text.Encoding.Unicode),
                Label = node.GetStringFromPath("IslandLabel", System.Text.Encoding.UTF8)
            };

            await island.InitAsync(region);
            return island;
        }

        public async Task UpdateExternalDataAsync()
        {
            if (AssumedMapPath is null)
                return;

            // fallback to read out map file
            int sizeInTiles = await FileDBReader.ReadTileInSizeFromFileAsync(AssumedMapPath);
            if (sizeInTiles != 0)
                SizeInTiles = sizeInTiles;

            if (Settings.Instance.DataPath is not null)
            {
                string activeMapImagePath = Path.Combine(Settings.Instance.DataPath, Path.GetDirectoryName(AssumedMapPath) ?? "", "_gamedata", Path.GetFileNameWithoutExtension(AssumedMapPath), "mapimage.png");
                ImageFile = activeMapImagePath;
            }
        }

        private async Task InitAsync(Region region)
        {
            if (ElementType == 2)
            {
                SizeInTiles = 1;
                return;
            }

            string? mapPath = MapPath;
            if (mapPath is not null && SpecialIslands.CachedSizes.ContainsKey(mapPath))
                SizeInTiles = SpecialIslands.CachedSizes[mapPath];

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

            if (SizeInTiles == 0)
                SizeInTiles = Size.InTiles;

            AssumedMapPath = mapPath;
            await UpdateExternalDataAsync();
        }
    }
}
