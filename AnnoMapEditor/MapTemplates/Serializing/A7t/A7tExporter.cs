using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t
{
    public class A7tExporter
    {
        public A7tExporter(int mapSize, int playableArea, Region mapRegion)
        {
            MapSize = mapSize;
            PlayableArea = playableArea;
            MapRegion = mapRegion;
        }

        private int MapSize { get; }
        private int PlayableArea { get; }
        private Region MapRegion { get; }

        public void ExportA7T(string a7tPath)
        {
            using (MemoryStream nestedDataStream = new MemoryStream())
            {
                //Create actual a7t File
                Gamedata gameDataItem = new Gamedata(MapSize, PlayableArea, MapRegion.AmbientName, true);

                FileDBDocumentSerializer serializer = new FileDBDocumentSerializer(new() { Version = FileDBDocumentVersion.Version1 });
                IFileDBDocument generatedFileDB = serializer.WriteObjectStructureToFileDBDocument(gameDataItem);

                using (Stream fileDbStream = new MemoryStream())
                {
                    DocumentWriter gamedataDocWriter = new DocumentWriter();
                    gamedataDocWriter.WriteFileDBToStream(generatedFileDB, fileDbStream);

                    if (fileDbStream.Position > 0)
                    {
                        fileDbStream.Seek(0, SeekOrigin.Begin);
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
