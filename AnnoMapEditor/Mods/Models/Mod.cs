using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Mods.Enums;
using AnnoMapEditor.Mods.Serialization;
using AnnoMapEditor.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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

namespace AnnoMapEditor.Mods.Models
{
    internal class Mod
    {
        public const string AME_POOL_PATH = @"data\ame\maps\pool";


        private Session _session;

        public MapType MapType { get; set; }


        public Mod(Session session)
        {
            this._session = session;
            MapType = MapType.Archipelago;
        }


        public async Task<bool> Save(string modPath, string modName, string? modID)
        {
            string fullModName = "[Map] " + modName;

            try
            {
                string? mapTypeFileName = MapType.ToFileName();
                string? mapTypeGuid = MapType.Guid;

                if (mapTypeFileName is null || mapTypeGuid is null)
                    throw new Exception("invalid MapType");

                if(!_session.Region.AllowModding)
                {
                    throw new Exception("not supported map region");
                }

                FileUtils.TryDeleteDirectory(modPath);
                Directory.CreateDirectory(modPath);
                await WriteMetaJson(modPath, modName, modID);

                //Only write Language XML for OW Maps, as only they need naming in a menu
                if(!string.IsNullOrEmpty(mapTypeGuid))
                    await WriteLanguageXml(modPath, modName, mapTypeGuid);

                string mapFilePath = Path.Combine(AME_POOL_PATH, _session.Region.PoolFolderName, mapTypeFileName);
                await WriteAssetsXml(modPath, fullModName, mapFilePath, _session.Region, MapType);

                //Create first entry with custom a7t and a7te
                List<string> sizes = _session.Region.GetAllSizeCombinations().ToList();
                string size = sizes[0];

                string basePath = Path.Combine(modPath, $"{mapFilePath}");
                await _session.SaveAsync(basePath + $"_{size}.a7tinfo", false);

                //a7t Creation
                string a7tPath = basePath + $"_{size}.a7t";
                await Task.Run(() => new A7tExporter(_session.Size.X, _session.PlayableArea.Width, _session.Region).ExportA7T(a7tPath));

                //a7te Creation
                string a7tePath = basePath + $"_{size}.a7te";
                await Task.Run(() => new A7teExporter(_session.Size.X).ExportA7te(a7tePath));


                if (_session.Region.HasMapExtension)
                {
                    await _session.SaveAsync(basePath + $"_{size}_enlarged.a7tinfo", true);
                    File.Copy(basePath + $"_{sizes[0]}.a7t", basePath + $"_{size}_enlarged.a7t");
                    File.Copy(basePath + $"_{sizes[0]}.a7te", basePath + $"_{size}_enlarged.a7te");
                }

                //copy a7t and a7te to remaining entries
                for (int i = 1; i<sizes.Count; i++)
                {
                    size = sizes[i];

                    await _session.SaveAsync(Path.Combine(modPath, $"{mapFilePath}_{size}.a7tinfo"), false);
                    File.Copy(basePath + $"_{sizes[0]}.a7t", basePath + $"_{size}.a7t");
                    File.Copy(basePath + $"_{sizes[0]}.a7te", basePath + $"_{size}.a7te");

                    if (_session.Region.HasMapExtension)
                    {
                        await _session.SaveAsync(basePath + $"_{size}_enlarged.a7tinfo", true);
                        File.Copy(basePath + $"_{sizes[0]}.a7t", basePath + $"_{size}_enlarged.a7t");
                        File.Copy(basePath + $"_{sizes[0]}.a7te", basePath + $"_{size}_enlarged.a7te");
                    }
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
                    $"World and island sizes are fixed.\n" +
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

        private async Task WriteAssetsXml(string modPath, string fullModName, string mapFilePath, Region region, MapType mapType)
        {
            string assetsXmlPath = Path.Combine(modPath, @"data\config\export\main\asset\assets.xml");
            string? assetsXmlDir = Path.GetDirectoryName(assetsXmlPath);
            if (assetsXmlDir is not null)
                Directory.CreateDirectory(assetsXmlDir);

            string fullMapPath = Path.Combine("mods", fullModName, mapFilePath).Replace("\\", "/");

            string content = CreateAssetsModOps(region, mapType, fullMapPath);

            using StreamWriter writer = new(File.Create(assetsXmlPath));
            await writer.WriteAsync(content);
        }


        public static bool CanSave(Session? session)
        {
            if (session is null)
                return false;

            return session.Region.AllowModding;
        }

        public static string CreateAssetsModOps(Region region, MapType mapType, string fullMapPath)
        {
            IEnumerable<string> sizes = region.MapSizes;
            // some maps have updates sizes, but make sure to only replace one
            IEnumerable<string> subSizes = region.MapSizeIndices;

            string MakeXPath(string mapTypeName, string size, string subsize)
            {
                if (string.IsNullOrEmpty(subsize))
                    return $"../Standard/Name='{mapTypeName}{size}'";
                else
                    return $"../Standard/Name='{mapTypeName}{size}_{subsize}'";
            }
            
            string content = "<ModOps>\n";

            if (region.UsesAllSizeIndices)
            {
                //Single ModOp for all Sub-Sizes
                foreach (var size in sizes)
                {
                    foreach(var subsize in subSizes)
                    {
                        content += CreateModOp(
                            new string[] { MakeXPath(mapType.ToName(), size, subsize) },
                            fullMapPath,
                            $"{size}_{subsize}",
                            region.HasMapExtension);
                    }
                }
            }
            else
            {
                //XPath OR
                foreach (var size in sizes)
                {
                    var xpaths = subSizes.Select(x => MakeXPath(mapType.ToName(), size, x));

                    content += CreateModOp(xpaths, fullMapPath, size, region.HasMapExtension);
                }
            }

            
            content += "</ModOps>\n";
            return content;
        }

        private static string CreateModOp(IEnumerable<string> xPaths, string basePath, string size, bool extension)
        {
            string result = CreateModOpSingle(xPaths, basePath, size, false);
            if (extension)
            {
                result += CreateModOpSingle(xPaths, basePath, size, true);
            }
            return result;
        }

        private static string CreateModOpSingle(IEnumerable<string> xPaths, string basePath, string size, bool extension)
        {
            const string TEMPLATE = "TemplateFilename";
            const string ENLARGED_TEMPLATE = "EnlargedTemplateFilename";

            string target = extension ? ENLARGED_TEMPLATE : TEMPLATE;

            string result = $"  <ModOp Type=\"replace\" Path=\"//MapTemplate[{string.Join(" or ", xPaths)}][last()]/{target}\">\n" +
                            $"    <{target}>{basePath}_{size}{(extension ? "_enlarged" : "")}.a7t</{target}>\n" + 
                            $"  </ModOp>\n";

            return result;
        }

        private static string MakeSafeName(string unsafeName) => new Regex(@"\W").Replace(unsafeName, "_").ToLower();
    }
}
