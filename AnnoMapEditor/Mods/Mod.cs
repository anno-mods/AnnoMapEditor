using AnnoMapEditor.MapTemplates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

/*
 * Modloader doesn't support a7t because they are loaded as .rda archive.
 * They specified with "mods/[Map] xyz/data/..."
 * Mistakes lead to endless loading.
 * 
 * The same map file path can't be used for differente TemplateSize at the same time.
 * Leads to endless loading.
 * I assume it's because first a pool is created, then maps are assigned to their group leading to an empty list for some groups.
 * 
 * corners_ll_01, snowflake_ll_01 are unused. do they work?
 */

namespace AnnoMapEditor.Mods
{
    internal class Mod
    {
        private Session session;

        public MapType MapType { get; set; }

        public Mod(Session session)
        {
            this.session = session;
            MapType = MapType.Archipelago;
        }

        public static bool CanSave(Session? session)
        {
            if (session is null)
                return false;

            string? sizeSourceMapName = ConvertSizeToMapName(session.PlayableArea.Width);
            if (sizeSourceMapName is null)
                return false;

            return true;
        }

        public async Task<bool> Save(string modPath, string modName, string? modID)
        {
            string fullModName = "[Map] " + modName;

            try
            {
                string? mapTypeFileName = MapType.ToName();
                string? mapTypeGuid = MapType.ToGuid();
                if (mapTypeFileName is null || mapTypeGuid is null)
                    throw new Exception("invalid MapType");

                string? sizeSourceMapName = ConvertSizeToMapName(session.PlayableArea.Width);
                if (sizeSourceMapName is null)
                    throw new Exception("not supported map size");

                Utils.TryDeleteDirectory(modPath);
                Directory.CreateDirectory(modPath);
                await WriteMetaJson(modPath, modName, modID);

                await WriteLanguageXml(modPath, modName, mapTypeGuid);

                string mapFilePath = $@"data\ame\maps\pool\moderate\{mapTypeFileName}";
                await WriteAssetsXml(modPath, fullModName, mapFilePath, MapType);

                string[] sizes = new[] { "ll", "lm", "ls", "ml", "mm", "ms", "sl", "sm", "ss" };
                foreach (var size in sizes)
                {
                    await session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}_{size}.a7tinfo"));
                    string sizeSourceMapPath = $@"data\sessions\maps\pool\moderate\{sizeSourceMapName}\{sizeSourceMapName}.a7t";
                    await CopyFromArchive(modPath, $"{sizeSourceMapPath}e", $"{mapFilePath}_{size}.a7te");
                    await CopyFromArchive(modPath, sizeSourceMapPath, $"{mapFilePath}_{size}.a7t");
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Failed to save the mod.\n\nIt looks like some files are locked, possibly by another application.\n\nThe mod may be broken now.", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to save the mod.\n\nThe mod may be broken now.", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        private async Task WriteMetaJson(string modPath, string modName, string? modID)
        {
            Modinfo? modinfo = null;

            string modinfoPath = Path.Combine(modPath, "modinfo.json");
            //if (File.Exists(modinfoPath))
            //{
            //    modinfo = JsonConvert.DeserializeObject<Modinfo?>(File.ReadAllText(modinfoPath));
            //}

            //if (modinfo is null)
            {
                modinfo = new()
                {
                    Version = "1",
                    ModID = string.IsNullOrEmpty(modID) ? $"ame_{MakeSafeName(modName)}_{Guid.NewGuid().ToString().Split('-').FirstOrDefault("")}" : modID,
                    ModName = new(modName),
                    Category = new("Map"),
                    Description = new($"Select Map Type '{modName}' to play this map.\n" +
                    $"World and  island sizes are fixed.\n" +
                    $"\n" +
                    $"Note:\n" +
                    $"- Do not rename the mod folder. It will lead to a loading screen freeze.\n" +
                    $"- You can combine map mods as long as they do not replace the same map type.\n" +
                    $"\n" +
                    $"This mod has been created with the {App.Title}.\n" +
                    $"You can download the editor at:\nhttps://github.com/anno-mods/AnnoMapEditor/releases/latest"),
                    CreatorName = App.TitleShort,
                    CreatorContact = "https://github.com/anno-mods/AnnoMapEditor"
                };
            }

            using StreamWriter writer = new(File.Create(modinfoPath));
            await writer.WriteAsync(JsonConvert.SerializeObject(modinfo, Formatting.Indented));
        }

        private async Task WriteLanguageXml(string modPath, string name, string guid)
        {
            string[] languages = new string[] { "chinese", "english", "french", "german", "italian", "japanese", "korean", "polish", "russian", "spanish", "taiwanese" };

            foreach (var language in languages)
            {
                string languageXmlPath = Path.Combine(modPath, $@"data\config\gui\texts_{language}.xml");
                string? languageXmlDir = Path.GetDirectoryName(languageXmlPath);
                if (languageXmlDir is not null)
                    Directory.CreateDirectory(languageXmlDir);

                string content =
                    $"<ModOps>\n" +
                    $"  <ModOp Type=\"replace\" Path=\"//TextExport/Texts/Text[GUID='{guid}']/Text\">\n" +
                    $"    <Text>{name}</Text>\n" +
                    $"  </ModOp>\n" +
                    $"</ModOps>\n";

                using StreamWriter writer = new(File.Create(languageXmlPath));
                await writer.WriteAsync(content);
            }
        }

        private async Task WriteAssetsXml(string modPath, string fullModName, string mapFilePath, MapType mapType)
        {
            string assetsXmlPath = Path.Combine(modPath, @"data\config\export\main\asset\assets.xml");
            string? assetsXmlDir = Path.GetDirectoryName(assetsXmlPath);
            if (assetsXmlDir is not null)
                Directory.CreateDirectory(assetsXmlDir);

            string fullMapPath = Path.Combine("mods", fullModName, mapFilePath).Replace("\\", "/");

            string content = CreateAssetsModOps(mapType, fullMapPath);

            using StreamWriter writer = new(File.Create(assetsXmlPath));
            await writer.WriteAsync(content);
        }

        public static string CreateAssetsModOps(MapType mapType, string fullMapPath)
        {
            string[] sizes = new[] { "ll", "lm", "ls", "ml", "mm", "ms", "sl", "sm", "ss" };
            // some maps have updates sizes, but make sure to only replace one
            string[] subSizes = new[] { "_01", "_02" };
            
            string content = "<ModOps>\n";

            foreach (var size in sizes)
            {
                var xpaths = subSizes.Select(x => $"../Standard/Name='{mapType.ToName()}_{size}{x}'");

                content +=
                $"  <ModOp Type=\"replace\" Path=\"//MapTemplate[{string.Join(" or ", xpaths)}][last()]/TemplateFilename\">\n" +
                $"    <TemplateFilename>{fullMapPath}_{size}.a7t</TemplateFilename>\n" +
                $"  </ModOp>\n";
            }
            content += "</ModOps>\n";
            return content;
        }

        private static string MakeSafeName(string unsafeName) => new Regex(@"\W").Replace(unsafeName, "_").ToLower();

        private class MapTemplateInfo
        {
            public string MapType = "";
            public string[] Templates = Array.Empty<string>();
        };

        private static string? ConvertSizeToMapName(int playableSize)
        {
            Dictionary<int, string[]> pairs = new()
            {
                [2160] = new[] { "moderate_atoll_ll_01", "moderate_atoll_lm_01", "moderate_atoll_ls_01",
                    "moderate_islandarc_ll_01", "moderate_islandarc_lm_01", "moderate_islandarc_ls_01" },
                [1656] = new[] { "moderate_archipel_ll_01", "moderate_archipel_lm_01", "moderate_archipel_ls_01",
                    "moderate_corners_lm_01", "moderate_corners_ll_02", "moderate_corners_ls_01",
                    "moderate_snowflake_ll_02", "moderate_snowflake_lm_01", "moderate_snowflake_ls_01" },
                [1648] = new[] { "moderate_atoll_ml_01", "moderate_atoll_mm_01", "moderate_atoll_ms_01",
                    "moderate_islandarc_ml_01", "moderate_islandarc_mm_01", "moderate_islandarc_ms_01" },
                [1440] = new[] { "moderate_corners_ml_01" },
                [1392] = new[] { "moderate_archipel_ml_01" },
                [1368] = new[] { "moderate_snowflake_mm_01" },
                [1360] = new[] { "moderate_corners_sl_01", "moderate_snowflake_ml_01" },
                [1352] = new[] { "moderate_snowflake_sm_01" },
                [1344] = new[] { "moderate_corners_sm_01" },
                [1336] = new[] { "moderate_atoll_sl_01", "moderate_atoll_sm_01", "moderate_atoll_ss_01",
                    "moderate_corners_mm_01", "moderate_corners_ms_01",
                    "moderate_islandarc_sl_01", "moderate_islandarc_sm_01", "moderate_islandarc_ss_01",
                    "moderate_snowflake_ms_01" },
                [1328] = new[] { "moderate_snowflake_sl_01" },
                [1312] = new[] { "moderate_archipel_mm_01", "moderate_archipel_ms_01" },
                [1224] = new[] { "moderate_archipel_sm_01" },
                [1208] = new[] { "moderate_corners_ss_01", "moderate_snowflake_ss_01" },
                [1200] = new[] { "moderate_archipel_sl_01", "moderate_archipel_ss_01" },
            };

            string[]? maps = pairs.GetValueOrDefault(playableSize);
            if (maps is null || maps.Length == 0)
                return null;

            return maps[0];
        }

        private async Task CopyFromArchive(string modPath, string sourcePath, string targetPath)
        {
            var archive = Settings.Instance.DataArchive;
            if (archive is null)
                return;

            using Stream? inStream = archive.OpenRead(sourcePath);
            if (inStream is not null)
            {
                using FileStream outStream = File.Create(Path.Combine(modPath, targetPath));
                await inStream.CopyToAsync(outStream);
            }
        }
    }
}
