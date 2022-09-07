using AnnoMapEditor.MapTemplates.Serializing.A7t.FileCreation;
using FileDBReader.src.XmlRepresentation;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t
{
    public class A7tExporter
    {
        public A7tExporter(int mapSize, int playableArea)
        {
            MapSize = mapSize;
            PlayableArea = playableArea;
        }

        private int MapSize { get; }
        private int PlayableArea { get; }

        [Obsolete("This hardcoded hexstring taken from a vanilla a7t File is here in case it is needed instead of the generated AreaManagerData. " +
            "It can safely be removed when the a7t is created by A7tDocumentExport Serialization.")]
        private const string vanillaAreaManagerDataString = 
                "020001800800000000000000000280080000000000000000030000000380000400050006000100048004010000000600000000000000000000000" +
                "70008000000000009000A00000000000B000A00000000000C000A0000000000000000000B456469746F724F626A656374000C004E617475726550" +
                "7265736574000B006F626A65637473000A004F626A65637447726F7570436F6C6C656374696F6E0007004F626A65637447726F757073000800466" +
                "96C74657200060047616D654F626A656374000900526F6F740005005175657565644368616E6765475549440003004F626A65637447726F757046" +
                "696C746572436F6C6C656374696F6E000400417265614F626A6563744D616E6167657200020004466F6C646572494400048051756575656444656" +
                "C657465730003804E6F6E47616D654F626A6563744944436F756E74657200028047616D654F626A6563744944436F756E7465720001805E000000";

        public void ExportA7T(string a7tPath)
        {
            using (MemoryStream nestedDataStream = new MemoryStream())
            {
                //Create nested AreaManagerData
                AreaManagerDataExport areaManagerDataExport = new AreaManagerDataExport();
                FileDBConvert.SerializeObject(areaManagerDataExport, new() { Version = FileDBDocumentVersion.Version1 }, nestedDataStream);

                //Create actual a7t File
                //TODO: Migrate from Obsolete XMLCreatorA7t to FileDBSerialization of A7tDocumentExport as soon as possible
                XMLCreatorA7T gameDataWriter = new XMLCreatorA7T(MapSize, PlayableArea, nestedDataStream.ToArray());

                using (Stream xmlStream = new MemoryStream())
                {
                    gameDataWriter.WriteXMLToStream(xmlStream);

                    if (xmlStream.Position > 0)
                    {
                        xmlStream.Position = 0;
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlStream);

                    XmlFileDbConverter<FileDBDocument_V1> converter = new();
                    IFileDBDocument doc = converter.ToFileDb(xmlDoc);

                    using (Stream fileDbStream = new MemoryStream())
                    {
                        DocumentWriter gamedataDocWriter = new DocumentWriter();
                        gamedataDocWriter.WriteFileDBToStream(doc, fileDbStream);

                        if (fileDbStream.Position > 0)
                        {
                            fileDbStream.Position = 0;
                        }


                        RDABlockCreator.FileType_CompressedExtensions.Add(".data");

                        using (RDAReader rdaReader = new RDAReader())
                        using (BinaryReader reader = new BinaryReader(fileDbStream))
                        {
                            RDAFolder rdaFolder = new RDAFolder(FileHeader.Version.Version_2_2);

                            rdaReader.rdaFolder = rdaFolder;
                            DirEntry gamedataFileDirEntry = new DirEntry()
                            {
                                filename = RDAFile.FileNameToRDAFileName("gamedata.data", ""),
                                offset = 0,
                                compressed = (ulong)fileDbStream.Length,
                                filesize = (ulong)fileDbStream.Length,
                                timestamp = RDAExplorer.Misc.DateTimeExtension.ToTimeStamp(DateTime.Now),
                            };

                            BlockInfo gamedataFileBlockInfo = new BlockInfo()
                            {
                                flags = 0,
                                fileCount = 1,
                                directorySize = (ulong)fileDbStream.Length,
                                decompressedSize = (ulong)fileDbStream.Length,
                                nextBlock = 0
                            };

                            RDAFile rdaFile = RDAFile.FromUnmanaged(FileHeader.Version.Version_2_2, gamedataFileDirEntry, gamedataFileBlockInfo, reader, null);
                            rdaFolder.AddFiles(new List<RDAFile>() { rdaFile });
                            RDAWriter writer = new RDAWriter(rdaFolder);
                            bool compress = true;
                            writer.Write(a7tPath, FileHeader.Version.Version_2_2, compress, rdaReader, null);

                        }

                        RDABlockCreator.FileType_CompressedExtensions.Remove(".data");
                    }

                }
            }
        }
    }
}
