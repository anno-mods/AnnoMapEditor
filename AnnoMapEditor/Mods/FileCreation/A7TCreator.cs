using FileDBReader.src.XmlRepresentation;
using FileDBSerializing;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using RDAExplorer;

namespace AnnoMapEditor.Mods.FileCreation
{
    internal class A7TCreator
    {

        public A7TCreator(int mapSize, int playable)
        {
            MapSize = mapSize;
            PlayableArea = playable;
        }

        private int MapSize { get; }
        private int PlayableArea { get; }

        public void CreateA7T(string fileName)
        {
            XMLCreatorA7T gameDataWriter = new XMLCreatorA7T(MapSize, PlayableArea);

            using (Stream stream = new MemoryStream())
            {
                gameDataWriter.WriteXMLToStream(stream);

                if(stream.Position > 0)
                {
                    stream.Position = 0;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                XmlFileDbConverter<FileDBDocument_V1> converter = new();
                IFileDBDocument doc = converter.ToFileDb(xmlDoc);

                DocumentWriter gamedataDocWriter = new DocumentWriter();
                string temp = Path.GetTempPath();
                string customTempFolder = Path.Combine(temp, "ame_temp");
                string gamedataFilePath = Path.Combine(customTempFolder, "gamedata.data");
                DirectoryInfo dir = Directory.CreateDirectory(customTempFolder);

                using (FileStream toTemp = new FileStream(gamedataFilePath, FileMode.Create))
                {
                    gamedataDocWriter.WriteFileDBToStream(doc, toTemp);
                }

                RDABlockCreator.FileType_CompressedExtensions.Add(".data");
                using (RDAReader someReader = new RDAReader())
                {
                    RDAFolder rdaFolder = new RDAFolder(FileHeader.Version.Version_2_2);

                    someReader.rdaFolder = rdaFolder;

                    RDAFile rdaFile = RDAFile.Create(FileHeader.Version.Version_2_2, gamedataFilePath, "");
                    rdaFolder.AddFiles(new List<RDAFile>() { rdaFile });
                    RDAWriter writer = new RDAWriter(rdaFolder);

                    bool compress = true;
                    writer.Write(fileName, FileHeader.Version.Version_2_2, compress, someReader, null);
                }

                RDABlockCreator.FileType_CompressedExtensions.Remove(".data");
            }
        }
    }
}
