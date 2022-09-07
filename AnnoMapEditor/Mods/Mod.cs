using AnnoMapEditor.MapTemplates;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using AnnoMapEditor.MapTemplates.Serializing.A7te;
using AnnoMapEditor.MapTemplates.Serializing.A7t;

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

            if (session.Region != Region.Moderate)
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

                if(session.Region != Region.Moderate)
                {
                    throw new Exception("not supported map region");
                }

                Utils.TryDeleteDirectory(modPath);
                Directory.CreateDirectory(modPath);
                await WriteMetaJson(modPath, modName, modID);

                await WriteLanguageXml(modPath, modName, mapTypeGuid);

                string mapFilePath = $@"data\ame\maps\pool\moderate\{mapTypeFileName}";
                await WriteAssetsXml(modPath, fullModName, mapFilePath, MapType);

                string[] sizes = new[] { "ll", "lm", "ls", "ml", "mm", "ms", "sl", "sm", "ss" };

                //Create first entry with custom a7t and a7te
                string size = sizes[0];
                string basePath = Path.Combine(modPath, $"{mapFilePath}");
                await session.SaveAsync(basePath + $"_{size}.a7tinfo");

                //a7t Creation
                string a7tPath = basePath + $"_{size}.a7t";
                await Task.Run(() => new A7tExporter(session.Size.X, session.PlayableArea.Width).ExportA7T(a7tPath));

                //a7te Creation
                string a7tePath = basePath + $"_{size}.a7te";
                await Task.Run(() => new A7teExporter(session.Size.X).ExportA7te(a7tePath));

                //copy a7t and a7te to remaining entries
                for (int i = 1; i<sizes.Length; i++)
                {
                    size = sizes[i];

                    await session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}_{size}.a7tinfo"));
                    File.Copy(basePath + $"_{sizes[0]}.a7t", basePath + $"_{size}.a7t");
                    File.Copy(basePath + $"_{sizes[0]}.a7te", basePath + $"_{size}.a7te");
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
            await writer.WriteAsync(JsonConvert.SerializeObject(modinfo, Newtonsoft.Json.Formatting.Indented));
        }

        private async Task WriteLanguageXml(string modPath, string name, string guid)
        {
            string languageXmlPath = Path.Combine(modPath, @"data\config\gui\texts_english.xml");
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
    }
}
