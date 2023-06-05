using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class SessionReader
    {
        public async Task<Session> FromDataArchiveAsync(string a7tinfoPath)
        {
            Region region = Region.DetectFromPath(a7tinfoPath);
            Stream a7tinfoStream = Settings.Instance!.DataArchive.OpenRead(a7tinfoPath)
                ?? throw new FileNotFoundException($"Could not find file \"{a7tinfoPath}\" in DataArchive.");

            return await FromBinaryStreamAsync(region, a7tinfoStream);
        }

        public async Task<Session> FromFileAsync(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (extension == "a7tinfo")
                return await FromBinaryFileAsync(filePath);
            else if (extension == "xml")
                return await FromXmlFileAsync(filePath);
            else
                throw new ArgumentException($"Unsupported extension {extension}. Expected either a7tinfo or xml.", nameof(filePath));
        }

        public async Task<Session> FromXmlFileAsync(string filePath)
        {
            Region region = Region.DetectFromPath(filePath);
            Stream a7tinfoXmlStream = File.OpenRead(filePath);
            return await FromXmlStreamAsync(region, a7tinfoXmlStream);
        }

        public async Task<Session> FromBinaryFileAsync(string filePath)
        {
            Region region = Region.DetectFromPath(filePath);
            Stream a7tinfoStream = File.OpenRead(filePath);
            return await FromBinaryStreamAsync(region, a7tinfoStream);
        }

        public async Task<Session> FromBinaryStreamAsync(Region region, Stream a7tinfoStream)
        {
            var doc = await FileDBSerializer.ReadAsync<MapTemplateDocument>(a7tinfoStream);
            if (doc is null)
                throw new Exception($"Could not read Session from binary stream.");

            return new Session(doc, region);
        }

        public async Task<Session> FromXmlStreamAsync(Region region, Stream a7tinfoXmlStream)
        {
            var doc = await FileDBSerializer.ReadFromXmlAsync<MapTemplateDocument>(a7tinfoXmlStream);
            if (doc is null)
                throw new Exception($"Could not read Session from XML stream.");

            return new Session(doc, region);
        }
    }
}
