using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Mods.Enums;
using AnnoMapEditor.Mods.Models;
using AnnoMapEditor.Utilities;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using Newtonsoft.Json;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AnnoMapEditor.Mods.Serialization
{
    public class ModWriter
    {
        public const string AME_POOL_PATH = @"data\ame\maps\pool";


        private readonly MapTemplateWriter _mapTemplateWriter;

        private readonly AssetRepository _assetRepository;


        public ModWriter()
        {
            _mapTemplateWriter = new();
            _assetRepository = Settings.Instance.AssetRepository
                ?? throw new Exception($"AssetRepository has not been initialized.");
        }


        private IList<MapTemplateAsset> GetMapTemplatesToReplace(SessionAsset session, MapType? mapType = null)
        {
            // Sessions may reference the MapTemplates to be used directly via the MapTemplate and
            // MapTemplateForMultiplayer attributes.
            if (session.MapTemplate != null)
            {
                List<MapTemplateAsset> mapTemplates = new();
                mapTemplates.Add(session.MapTemplate);
                if (session.MapTemplateForMultiplayer != null)
                    mapTemplates.Add(session.MapTemplateForMultiplayer);

                return mapTemplates;
            }

            // Otherwise get pool maps matching the Session's region.
            else
            {
                IEnumerable<MapTemplateAsset> mapTemplates = _assetRepository.GetAll<MapTemplateAsset>()
                    .Where(m => m.TemplateRegion == session.Region)
                    .Where(m => m.TemplateFilename.Contains("pool"));

                // For The Old World filter according to the MapType.
                if (session == SessionAsset.OldWorld)
                {
                    if (mapType != null)
                        mapTemplates = mapTemplates.Where(m => m.TemplateMapType == mapType);
                    else
                        throw new ArgumentException($"Cannot determine MapTemplates to replace for session {session.GUID} \"{session.DisplayName}\" without a given {typeof(MapType).FullName}.");
                }

                return mapTemplates.ToList();
            }
        }

        public async Task<bool> WriteAsync(Mod mod, string modPath, string modName, string? modID)
        {
            FileUtils.TryDeleteDirectory(modPath);
            Directory.CreateDirectory(modPath);

            SessionAsset session = mod.MapTemplate.Session;

            IList<MapTemplateAsset> mapTemplatesToReplace = GetMapTemplatesToReplace(session, mod.MapType);

            await WriteModinfoJson(modPath, modName, modID);

            //Only write Language XML for OW Maps, as only they need naming in a menu
            if (session == SessionAsset.OldWorld)
                await WriteLanguageXml(modPath, modName, mod.MapType.Guid);

            // create the first copy of a7t, a7tinfo and a7te
            MapTemplateAsset firstMapTemplate = mapTemplatesToReplace.First();
            string mapFileDirectory = Path.Combine(modPath, AME_POOL_PATH);
            string a7tPath = Path.Combine(mapFileDirectory, Path.GetFileName(firstMapTemplate.TemplateFilename));
            string a7tinfoPath = Path.ChangeExtension(a7tPath, "a7tinfo");
            string a7tePath = Path.ChangeExtension(a7tPath, "a7te");

            await _mapTemplateWriter.WriteA7tinfoAsync(mod.MapTemplate, Path.Combine(modPath, a7tinfoPath));
            WriteA7T(mod, a7tPath);
            WriteA7te(mod.MapTemplate.Size.X, a7tePath);

            // copy a7t, a7tinfo and a7te for all MapTemplates that must be replaced
            for (int i = 1; i < mapTemplatesToReplace.Count; ++i)
            {
                MapTemplateAsset mapTemplate = mapTemplatesToReplace[i];
                string mapFilename = Path.GetFileNameWithoutExtension(mapTemplate.TemplateFilename);

                File.Copy(a7tPath, Path.Combine(mapFileDirectory, mapFilename + ".a7t"));
                File.Copy(a7tinfoPath, Path.Combine(mapFileDirectory, mapFilename + ".a7tinfo"));
                File.Copy(a7tePath, Path.Combine(mapFileDirectory, mapFilename + ".a7te"));

                if (mapTemplate.EnlargedTemplateFilename != null)
                {
                    File.Copy(a7tPath, Path.Combine(mapFileDirectory, mapFilename + "_enlarged.a7t"));
                    File.Copy(a7tinfoPath, Path.Combine(mapFileDirectory, mapFilename + "_enlarged.a7tinfo"));
                    File.Copy(a7tePath, Path.Combine(mapFileDirectory, mapFilename + "_enlarged.a7te"));
                }
            }

            await WriteAssetsXml(modPath, mapTemplatesToReplace);

            return true;
        }

        private static async Task WriteAssetsXml(string modPath, IEnumerable<MapTemplateAsset> mapTemplatesToReplace)
        {
            string assetsXmlPath = Path.Combine(modPath, @"data\config\export\main\asset\assets.xml");

            string? assetsXmlDir = Path.GetDirectoryName(assetsXmlPath);
            if (assetsXmlDir is not null)
                Directory.CreateDirectory(assetsXmlDir);

            StringBuilder sb = new();
            sb.Append("<ModOps>\n");

            foreach (MapTemplateAsset mapTemplateAsset in mapTemplatesToReplace)
            {
                string templateFilename = Path.Combine(AME_POOL_PATH, Path.GetFileName(mapTemplateAsset.TemplateFilename));
                string? enlargedTemplateFilename = mapTemplateAsset.EnlargedTemplateFilename != null ? Path.Combine(AME_POOL_PATH, Path.GetFileName(mapTemplateAsset.EnlargedTemplateFilename)) : null;

                WriteModOpReplaceTemplateFilename(sb, mapTemplateAsset.GUID, templateFilename, enlargedTemplateFilename);

                if (mapTemplateAsset.EnlargedTemplateFilename != null)
                    WriteModOpReplaceTemplateFilename(sb, mapTemplateAsset.GUID, templateFilename, enlargedTemplateFilename);
            }

            sb.Append("</ModOps>\n");

            using StreamWriter writer = new(File.Create(assetsXmlPath));
            await writer.WriteAsync(sb.ToString());
        }

        private static async Task WriteModinfoJson(string modPath, string modName, string? modID)
        {
            Modinfo modinfo = new()
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

            string modinfoPath = Path.Combine(modPath, "modinfo.json");

            using StreamWriter writer = new(File.Create(modinfoPath));
            await writer.WriteAsync(JsonConvert.SerializeObject(modinfo, Newtonsoft.Json.Formatting.Indented));
        }

        private static void WriteModOpReplaceTemplateFilename(StringBuilder sb, long mapTemplateGuid, string templateFilename, string? enlargedTemplateFilename)
        {
            sb.Append($"  <ModOp GUID=\"{mapTemplateGuid}\" Type=\"merge\" Path=\"/Values/MapTemplate\">\n")
              .Append($"    <TemplateFilename>{templateFilename.Replace('\\', '/')}</TemplateFilename>\n");

            if (enlargedTemplateFilename != null)
                sb.Append($"    <EnlargedTemplateFilename>{templateFilename.Replace('\\', '/')}</EnlargedTemplateFilename>\n");

            sb.Append($"  </ModOp>\n");
        }


        private static void WriteA7T(Mod mod, string a7tPath)
        {
            using MemoryStream nestedDataStream = new();

            // If the session has existing Gamedata, it must be updated to reflect all changes made within the AME.
            Gamedata gameDataItem = new(mod.MapTemplate.Size.X, mod.MapTemplate.PlayableArea.Width, mod.MapTemplate.Session.Region.Ambiente!, true);

            //Create actual a7t File
            FileDBDocumentSerializer serializer = new(new() { Version = FileDBDocumentVersion.Version1 });
            IFileDBDocument generatedFileDB = serializer.WriteObjectStructureToFileDBDocument(gameDataItem);

            using MemoryStream fileDbStream = new();

            DocumentWriter gamedataDocWriter = new();
            gamedataDocWriter.WriteFileDBToStream(generatedFileDB, fileDbStream);

            if (fileDbStream.Position > 0)
            {
                fileDbStream.Seek(0, SeekOrigin.Begin);
            }

            RDABlockCreator.FileType_CompressedExtensions.Add(".data");

            using (RDAReader rdaReader = new())
            using (BinaryReader reader = new(fileDbStream))
            {
                RDAFolder rdaFolder = new(FileHeader.Version.Version_2_2);

                rdaReader.rdaFolder = rdaFolder;
                DirEntry gamedataFileDirEntry = new()
                {
                    filename = RDAFile.FileNameToRDAFileName("gamedata.data", ""),
                    offset = 0,
                    compressed = (ulong)fileDbStream.Length,
                    filesize = (ulong)fileDbStream.Length,
                    timestamp = RDAExplorer.Misc.DateTimeExtension.ToTimeStamp(DateTime.Now),
                };

                BlockInfo gamedataFileBlockInfo = new()
                {
                    flags = 0,
                    fileCount = 1,
                    directorySize = (ulong)fileDbStream.Length,
                    decompressedSize = (ulong)fileDbStream.Length,
                    nextBlock = 0
                };

                RDAFile rdaFile = RDAFile.FromUnmanaged(FileHeader.Version.Version_2_2, gamedataFileDirEntry, gamedataFileBlockInfo, reader, null);
                rdaFolder.AddFiles(new List<RDAFile>() { rdaFile });
                RDAWriter writer = new(rdaFolder);
                bool compress = true;
                writer.Write(a7tPath, FileHeader.Version.Version_2_2, compress, rdaReader, null);

            }

            RDABlockCreator.FileType_CompressedExtensions.Remove(".data");
        }

        private static void WriteA7te(int mapRadius, string a7tePath)
        {
            AnnoEditorLevel a7te = new(mapRadius);

            XmlSerializer a7teSerializer = new(typeof(AnnoEditorLevel));
            XmlWriterSettings xmlSettings = new() { Indent = true, IndentChars = "  ", OmitXmlDeclaration = true, Async = true };
            XmlSerializerNamespaces noNamespaces = new(new XmlQualifiedName[] { XmlQualifiedName.Empty });

            using StreamWriter streamWriter = new(a7tePath, false, Encoding.UTF8);
            using XmlWriter xmlWriter = XmlWriter.Create(streamWriter, xmlSettings);

            a7teSerializer.Serialize(xmlWriter, a7te, noNamespaces);
        }

        private static async Task WriteLanguageXml(string modPath, string name, string guid)
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


        private static string MakeSafeName(string unsafeName) => new Regex(@"\W").Replace(unsafeName, "_").ToLower();
    }
}
