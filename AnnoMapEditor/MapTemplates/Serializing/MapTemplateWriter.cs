using AnnoMapEditor.MapTemplates.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class MapTemplateWriter
    {
        public async Task WriteXmlAsync(MapTemplate mapTemplate, string filePath)
        {
            var export = mapTemplate.ToTemplateDocument()
                ?? throw new NullReferenceException($"Failed to write MapTemplate to XML. The MapTemplate could not be translated to a MapTemplateDocument.");

            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await FileDBSerializer.WriteToXmlAsync(export, file);
        }

        public async Task WriteA7tinfoAsync(MapTemplate mapTemplate, string filePath)
        {
            var export = mapTemplate.ToTemplateDocument()
                ?? throw new NullReferenceException($"Failed to write MapTemplate to a7tinfo. The MapTemplate could not be translated to a MapTemplateDocument.");

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await FileDBSerializer.WriteAsync(export, file);
        }

    }
}
