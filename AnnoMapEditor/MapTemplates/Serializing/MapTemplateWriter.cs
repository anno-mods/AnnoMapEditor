using AnnoMapEditor.MapTemplates.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class MapTemplateWriter
    {
        public async Task WriteAsync(MapTemplate mapTemplate, string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                await WriteA7tinfoAsync(mapTemplate, filePath);
            else if (Path.GetExtension(filePath).ToLower() == ".xml")
                await WriteXmlAsync(mapTemplate, filePath);
            else
                throw new ArgumentException();
        }

        public async Task WriteA7tinfoAsync(MapTemplate mapTemplate, string filePath)
        {
            var export = mapTemplate.ToTemplate();
            if (export is null)
                throw new Exception("Attempted to save empty MapTemplate.");

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);

            // clear any existing file
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0);

            await Serializer.WriteAsync(export, file);
        }

        public async Task WriteXmlAsync(MapTemplate mapTemplate, string filePath)
        {
            var export = mapTemplate.ToTemplate();
            if (export is null)
                throw new Exception("Attempted to save empty MapTemplate.");

            // clear any existing file
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0);

            await Serializer.WriteToXmlAsync(export, file);
        }
    }
}
