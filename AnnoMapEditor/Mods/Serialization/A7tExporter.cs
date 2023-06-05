using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared;
using AnnoMapEditor.DataArchives.Assets.Models;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;

namespace AnnoMapEditor.Mods.Serialization
{
    public class A7tExporter
    {
        private readonly int _mapSize;

        private readonly int _playableArea;

        private readonly RegionAsset _mapRegion;


        public A7tExporter(int mapSize, int playableArea, RegionAsset mapRegion)
        {
            _mapSize = mapSize;
            _playableArea = playableArea;
            _mapRegion = mapRegion;
        }


        public void ExportA7T(string a7tPath)
        {
            using MemoryStream nestedDataStream = new();

            //Create actual a7t File
            Gamedata gameDataItem = new(_mapSize, _playableArea, _mapRegion.Ambiente!, true);

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
    }
}
