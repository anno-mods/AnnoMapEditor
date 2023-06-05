using AnnoMapEditor.MapTemplates.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class SessionWriter
    {
        public async Task WriteAsync(Session session, string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                await WriteA7tinfoAsync(session, filePath);
            else if (Path.GetExtension(filePath).ToLower() == ".xml")
                await WriteXmlAsync(session, filePath);
            else
                throw new ArgumentException();
        }

        public async Task WriteA7tinfoAsync(Session session, string filePath)
        {
            var export = session.ToTemplate();
            if (export is null)
                throw new Exception("Attempted to save empty session.");

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);

            // clear any existing file
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0);

            await Serializer.WriteAsync(export, file);
        }

        public async Task WriteXmlAsync(Session session, string filePath)
        {
            var export = session.ToTemplate();
            if (export is null)
                throw new Exception("Attempted to save empty session.");

            // clear any existing file
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0);

            await Serializer.WriteToXmlAsync(export, file);
        }
    }
}
