using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared;
using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoRDA.Loader;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MapTemplate = AnnoMapEditor.MapTemplates.Models.MapTemplate;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class MapTemplateReader
    {
        private const string A7T_EXTENSION = "a7t";


        public async Task<MapTemplate> FromDataArchiveAsync(string a7tinfoPath)
        {
            SessionAsset session = SessionAsset.DetectFromPath(a7tinfoPath);
            Stream a7tinfoStream = DataManager.Instance.DataArchive.OpenRead(a7tinfoPath)
                ?? throw new FileNotFoundException($"Could not find file \"{a7tinfoPath}\" in DataArchive.");

            Stream? gamedataStream = null;
            try
            {
                string gamedataPath = Path.ChangeExtension(a7tinfoPath, A7T_EXTENSION) + "|gamedata.data";
                gamedataStream  = DataManager.Instance.DataArchive.OpenRead(gamedataPath)!;
            }
            catch
            {
                // TODO: Log Warning
            }

            return await FromBinaryStreamAsync(session, a7tinfoStream, gamedataStream);
        }

        public async Task<MapTemplate> FromFileAsync(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension == ".a7tinfo")
                return await FromBinaryFileAsync(filePath);
            else if (extension == ".xml")
                return await FromXmlFileAsync(filePath);
            else
                throw new ArgumentException($"Unsupported extension {extension}. Expected either a7tinfo or xml.", nameof(filePath));
        }

        public async Task<MapTemplate> FromXmlFileAsync(string a7tinfoPath)
        {
            SessionAsset session = SessionAsset.DetectFromPath(a7tinfoPath);
            Stream a7tinfoXmlStream = File.OpenRead(a7tinfoPath);

            Stream? a7tStream = null;
            try
            {
                string a7tPath = Path.ChangeExtension(a7tinfoPath, A7T_EXTENSION);
                a7tStream = File.OpenRead(a7tPath);
            }
            catch
            {
                // TODO: Log Warning
            }

            return await FromXmlStreamAsync(session, a7tinfoXmlStream, a7tStream);
        }

        public async Task<MapTemplate> FromBinaryFileAsync(string a7tinfoPath)
        {
            SessionAsset session = SessionAsset.DetectFromPath(a7tinfoPath);
            Stream a7tinfoStream = File.OpenRead(a7tinfoPath);

            Stream? gamedataStream = null;
            try
            {
                string a7tPath = Path.ChangeExtension(a7tinfoPath, A7T_EXTENSION);
                RdaArchiveLoader loader = new();
                AnnoRDA.FileSystem a7tFs = loader.Load(a7tPath);
                gamedataStream = a7tFs.OpenRead("gamedata.data");
            }
            catch
            {
                // TODO: Log Warning
            }

            return await FromBinaryStreamAsync(session, a7tinfoStream, gamedataStream);
        }

        public async Task<MapTemplate> FromBinaryStreamAsync(SessionAsset session, Stream a7tinfoStream, Stream? gamedataStream)
        {
            var doc = await FileDBSerializer.ReadAsync<MapTemplateDocument>(a7tinfoStream);
            if (doc is null)
                throw new Exception($"Could not read MapTemplate from binary stream.");

            Gamedata? gamedata = null;
            if (gamedataStream != null)
            {
                // If gamedata.data is read directly from a .a7t-file, it is most compressed and
                // a7tStream is a ZlibStream wrapping a DeflateStream. DeflateStreams are not
                // seekable. This breaks deserialization in Serializer.ReadAsync.
                if (!gamedataStream.CanSeek)
                {
                    Stream origA7tStream = gamedataStream;
                    gamedataStream = new MemoryStream();
                    origA7tStream.CopyTo(gamedataStream);
                }

                gamedata = await FileDBSerializer.ReadAsync<Gamedata>(gamedataStream);
            }

             return new MapTemplate(session, doc, gamedata);
        }

        public async Task<MapTemplate> FromXmlStreamAsync(SessionAsset session, Stream a7tinfoXmlStream, Stream? a7tStream)
        {
            var doc = await FileDBSerializer.ReadFromXmlAsync<MapTemplateDocument>(a7tinfoXmlStream);
            if (doc is null)
                throw new Exception($"Could not read MapTemplate from XML stream.");

            Gamedata? gamedata = null;
            if (a7tStream != null)
            {
                gamedata = await FileDBSerializer.ReadFromXmlAsync<Gamedata>(a7tStream);
            }

            return new MapTemplate(session, doc, gamedata);
        }
    }
}
